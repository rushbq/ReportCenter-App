using Microsoft.AspNetCore.Authentication.Negotiate;
using ReportCenter.Web.Middleware;
using ReportCenter.Web.Models.Settings;
using ReportCenter.Web.Repositories;
using ReportCenter.Web.Services;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    {
        var isDevelopment = context.HostingEnvironment.IsDevelopment();
        var seqServerUrl = context.Configuration["Seq:ServerUrl"];
        var seqApiKey = context.Configuration["Seq:ApiKey"];

        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "ReportCenter.Web")
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);

        if (isDevelopment)
        {
            loggerConfiguration.WriteTo.Console(
                outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
            return;
        }

        if (string.IsNullOrWhiteSpace(seqServerUrl))
        {
            throw new InvalidOperationException("Production 環境缺少 Seq:ServerUrl 設定，無法輸出例外與 request log。");
        }

        loggerConfiguration.WriteTo.Seq(seqServerUrl, apiKey: string.IsNullOrWhiteSpace(seqApiKey) ? null : seqApiKey);
    });

    // Add services to the container.
    builder.Services.AddRazorPages();
    builder.Services.AddHttpContextAccessor();

    // 設定 (appsettings.json)
    builder.Services.Configure<ReportBaseUrlSettings>(
        builder.Configuration.GetSection(ReportBaseUrlSettings.SectionName));
    builder.Services.Configure<DepartmentDisplaySettings>(
        builder.Configuration.GetSection(DepartmentDisplaySettings.SectionName));

    // ── Windows 驗證設定 ──────────────────────────────
    if (builder.Environment.IsDevelopment())
    {
        // 開發環境：模擬 Windows AD 身份（macOS 無法使用 Windows 驗證）
        builder.Services.Configure<MockWindowsAuthSettings>(
            builder.Configuration.GetSection(MockWindowsAuthSettings.SectionName));
    }
    else
    {
        // 正式環境 (IIS)：啟用 Negotiate (Windows) 驗證
        builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
            .AddNegotiate();
        builder.Services.AddAuthorization();
    }

    // 三層式架構 DI 註冊
    // DAL — 資料存取層
    builder.Services.AddScoped<ICatalogRepository, SqlCatalogRepository>();

    // BLL — 商業邏輯層
    builder.Services.AddScoped<IReportService, SqlReportService>();
    builder.Services.AddScoped<ICatalogService, CatalogService>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error");
    }

    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);
            diagnosticContext.Set("RequestPath", httpContext.Request.Path.Value ?? string.Empty);
            diagnosticContext.Set("Method", httpContext.Request.Method);
            diagnosticContext.Set("UserName", httpContext.User.Identity?.Name ?? "Anonymous");
        };
    });

    app.UseRouting();

    // ── 驗證中介層 ──────────────────────────────────
    if (app.Environment.IsDevelopment())
    {
        // 開發環境：注入模擬 Windows AD 身份
        app.UseMiddleware<DevWindowsAuthMiddleware>();
    }
    else
    {
        // 正式環境：由 IIS 處理 Windows 驗證
        app.UseAuthentication();
    }

    app.UseAuthorization();
    app.UseMiddleware<UnhandledExceptionLoggingMiddleware>();

    app.MapStaticAssets();
    app.MapRazorPages()
       .WithStaticAssets();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "應用程式啟動失敗");
}
finally
{
    Log.CloseAndFlush();
}
