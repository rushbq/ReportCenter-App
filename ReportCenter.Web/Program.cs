using Microsoft.AspNetCore.Authentication.Negotiate;
using ReportCenter.Web.Middleware;
using ReportCenter.Web.Models.Settings;
using ReportCenter.Web.Repositories;
using ReportCenter.Web.Services;
using Serilog;
using Serilog.Events;

// ── Bootstrap Logger ─────────────────────────────────────────
// 應用程式啟動階段尚無 Host，用精簡 Bootstrap Logger 擷取啟動失敗
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── 外部設定檔 (正式環境透過 IIS web.config 環境變數指定路徑) ──
    // 載入順序：共用基礎設定 → 站台專用設定 (後者覆蓋前者)
    if (!builder.Environment.IsDevelopment())
    {
        var sharedInfraPath = Environment.GetEnvironmentVariable("SHARED_INFRA_PATH");
        if (!string.IsNullOrEmpty(sharedInfraPath))
            builder.Configuration.AddJsonFile(sharedInfraPath, optional: false, reloadOnChange: false);

        var siteConfigPath = Environment.GetEnvironmentVariable("RC_CONFIG_PATH");
        if (!string.IsNullOrEmpty(siteConfigPath))
            builder.Configuration.AddJsonFile(siteConfigPath, optional: false, reloadOnChange: false);
    }

    // ── Serilog 主設定 ───────────────────────────────────────
    builder.Host.UseSerilog((context, services, cfg) =>
    {
        var env = context.HostingEnvironment;

        // 共用基底：最低等級、Enricher、屬性
        cfg.ReadFrom.Configuration(context.Configuration)
           .ReadFrom.Services(services)
           .Enrich.FromLogContext()
           .Enrich.WithProperty("Application", "ReportCenter.Web")
           .Enrich.WithProperty("Environment", env.EnvironmentName)
           .Enrich.WithProperty("MachineName", System.Environment.MachineName);

        // ── 環境專屬 Sink ────────────────────────────────────
        if (env.IsDevelopment())
        {
            // 開發：僅 Console，精簡格式方便本機除錯
            cfg.MinimumLevel.Debug()
               .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
               .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
               .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
               .WriteTo.Console(outputTemplate:
                   "{Timestamp:HH:mm:ss} [{Level:u3}] {SourceContext}{NewLine}" +
                   "  → {Message:lj}{NewLine}{Exception}");
        }
        else
        {
            // Staging / Production：File + Seq
            cfg.MinimumLevel.Information()
               .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
               .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
               .MinimumLevel.Override("Dapper", LogEventLevel.Warning);

            // File Sink — logs/rc-YYYYMMDD.log，每日滾動，保留 30 天
            var logPath = Path.Combine(env.ContentRootPath, "logs", "rc-.log");
            cfg.WriteTo.File(
                path: logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                fileSizeLimitBytes: 50 * 1024 * 1024,   // 單檔 50 MB
                rollOnFileSizeLimit: true,
                shared: true,
                outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] " +
                    "TraceId={TraceId} User={UserName} " +
                    "{Message:lj}{NewLine}{Exception}");

            // Seq Sink (選配：有設定才啟用)
            var seqUrl = context.Configuration["Seq:ServerUrl"];
            var seqKey = context.Configuration["Seq:ApiKey"];
            if (!string.IsNullOrWhiteSpace(seqUrl))
            {
                cfg.WriteTo.Seq(seqUrl,
                    apiKey: string.IsNullOrWhiteSpace(seqKey) ? null : seqKey,
                    restrictedToMinimumLevel: LogEventLevel.Information);
            }
        }
    });

    // ── 服務註冊 ─────────────────────────────────────────────
    builder.Services.AddRazorPages();
    builder.Services.AddHttpContextAccessor();

    // 設定 (appsettings.json)
    builder.Services.Configure<ReportBaseUrlSettings>(
        builder.Configuration.GetSection(ReportBaseUrlSettings.SectionName));
    builder.Services.Configure<DepartmentDisplaySettings>(
        builder.Configuration.GetSection(DepartmentDisplaySettings.SectionName));

    // ── Windows 驗證設定 ─────────────────────────────────────
    if (builder.Environment.IsDevelopment())
    {
        builder.Services.Configure<MockWindowsAuthSettings>(
            builder.Configuration.GetSection(MockWindowsAuthSettings.SectionName));
    }
    else
    {
        builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
            .AddNegotiate();
        builder.Services.AddAuthorization();
    }

    // 三層式架構 DI 註冊
    // DAL — 資料存取層
    builder.Services.AddScoped<ICatalogRepository, SqlCatalogRepository>();
    builder.Services.AddScoped<IPksysRepository, SqlPksysRepository>();
    builder.Services.AddScoped<IPermissionRepository, SqlPermissionRepository>();

    // BLL — 商業邏輯層
    builder.Services.AddScoped<IReportService, SqlReportService>();
    builder.Services.AddScoped<ICatalogService, CatalogService>();
    builder.Services.AddScoped<IPermissionService, PermissionService>();

    var app = builder.Build();

    // ── HTTP Pipeline ────────────────────────────────────────
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error");
    }

    // Serilog Request Logging — 自動記錄每筆 HTTP 請求
    app.UseSerilogRequestLogging(options =>
    {
        // 靜態資源不記錄 (css/js/images/favicon)
        options.GetLevel = (httpContext, elapsed, ex) =>
        {
            var path = httpContext.Request.Path.Value ?? "";
            if (path.StartsWith("/css") || path.StartsWith("/js") ||
                path.StartsWith("/images") || path.StartsWith("/favicon"))
                return LogEventLevel.Verbose;

            if (ex != null) return LogEventLevel.Error;
            if (httpContext.Response.StatusCode >= 500) return LogEventLevel.Error;
            if (httpContext.Response.StatusCode >= 400) return LogEventLevel.Warning;
            if (elapsed > 3000) return LogEventLevel.Warning;   // 慢請求 > 3s

            return LogEventLevel.Information;
        };

        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);
            diagnosticContext.Set("UserName", httpContext.User.Identity?.Name ?? "Anonymous");
            diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString() ?? "-");
        };
    });

    app.UseRouting();

    // ── 驗證中介層 ───────────────────────────────────────────
    if (app.Environment.IsDevelopment())
    {
        app.UseMiddleware<DevWindowsAuthMiddleware>();
    }
    else
    {
        app.UseAuthentication();
    }

    app.UseAuthorization();
    app.UseMiddleware<UnhandledExceptionLoggingMiddleware>();

    app.MapStaticAssets();
    app.MapRazorPages()
       .WithStaticAssets();

    Log.Information("ReportCenter.Web 啟動完成 [{Environment}]", app.Environment.EnvironmentName);
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
