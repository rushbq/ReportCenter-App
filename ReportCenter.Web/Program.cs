using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
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
        LoadExternalConfig(builder.Configuration, "SHARED_INFRA_PATH", "共用機密設定檔");
        LoadExternalConfig(builder.Configuration, "RC_CONFIG_PATH", "站台機密設定檔");
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
    builder.Services.AddMemoryCache();   // 首頁戰情快取 (HomeDashboardService)

    // 設定 (appsettings.json)
    builder.Services.Configure<SiteSettings>(
        builder.Configuration.GetSection(SiteSettings.SectionName));
    builder.Services.Configure<ReportBaseUrlSettings>(
        builder.Configuration.GetSection(ReportBaseUrlSettings.SectionName));
    // DeptID 必須為數字：ReportDepartment.DeptID 為 INT，非數字代碼會無法對應任何報表。
    // 啟動時即驗證 (ValidateOnStart)，設定錯誤直接啟動失敗並寫入 Fatal log，不讓問題靜默發生。
    builder.Services.AddOptions<DepartmentDisplaySettings>()
        .Bind(builder.Configuration.GetSection(DepartmentDisplaySettings.SectionName))
        .Validate(s => s.Regions.Values.SelectMany(r => r.Depts).All(d => int.TryParse(d.DeptID, out _)),
            "DepartmentDisplay 設定中的 DeptID 必須為數字 (ReportDepartment.DeptID 為 INT)")
        .ValidateOnStart();
    builder.Services.Configure<WindowsAuthDebugSettings>(
        builder.Configuration.GetSection(WindowsAuthDebugSettings.SectionName));

    // ── 登入驗證 (DataHubApi JWT → 本站 Cookie) ──────────────
    // 帳密與 Refresh Token 由 DataHubApi 負責 (與 PKPortal 共用，在任一站登入過即可免登)；
    // 登入頁取得 JWT 後交給本站驗證，再發本站自己的 HttpOnly Cookie。規格見 docs/內部站台登入規格.md
    if (builder.Environment.IsDevelopment())
    {
        builder.Services.Configure<MockWindowsAuthSettings>(
            builder.Configuration.GetSection(MockWindowsAuthSettings.SectionName));
    }

    builder.Services.AddOptions<DataHubAuthSettings>()
        .Bind(builder.Configuration.GetSection(DataHubAuthSettings.SectionName))
        .Validate(s => builder.Environment.IsDevelopment() || s.IsValidForProduction(),
            "DataHubAuth 設定錯誤：ApiBaseUrl 必須是 https 網址，PublicKeyPem 必須是 DataHubApi 的 RSA 公鑰")
        .ValidateOnStart();
    builder.Services.AddSingleton<IDataHubTokenValidator, DataHubTokenValidator>();

    var cookieHours = builder.Configuration.GetValue($"{DataHubAuthSettings.SectionName}:CookieHours", 8);
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.Cookie.Name = "rc_auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            // 本機 http://localhost 開發時 Safari 不收 Secure Cookie，其餘環境一律 Secure
            options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;
            options.ExpireTimeSpan = TimeSpan.FromHours(cookieHours);
            options.SlidingExpiration = true;
            options.LoginPath = "/Account/Login";
            // 只有一般換頁才轉到登入頁；fetch / sendBeacon 等請求回 401，由前端重新整理頁面走自動登入
            options.Events.OnRedirectToLogin = ctx =>
            {
                if (IsPageNavigation(ctx.Request))
                    ctx.Response.Redirect(ctx.RedirectUri);
                else
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
        });

    // 全站預設須登入 (Fail-closed)；登入頁、錯誤頁、靜態檔以 [AllowAnonymous] 排除
    builder.Services.AddAuthorization(options =>
        options.FallbackPolicy = options.DefaultPolicy);

    // Cookie 與 Antiforgery Token 以 Data Protection 金鑰加密；金鑰須存到固定資料夾，
    // 否則應用程式集區回收後所有人都會被登出
    var dataProtection = builder.Services.AddDataProtection().SetApplicationName("ReportCenter");
    var keysPath = builder.Configuration["DataProtection:KeysPath"];
    if (!string.IsNullOrWhiteSpace(keysPath))
    {
        dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keysPath));
        if (OperatingSystem.IsWindows())
            dataProtection.ProtectKeysWithDpapi(protectToLocalMachine: true);
    }

    // 三層式架構 DI 註冊
    // DAL — 資料存取層
    builder.Services.AddScoped<ICatalogRepository, SqlCatalogRepository>();
    builder.Services.AddScoped<IPksysRepository, SqlPksysRepository>();
    builder.Services.AddScoped<IPermissionRepository, SqlPermissionRepository>();
    builder.Services.AddScoped<IUsageRepository, SqlUsageRepository>();

    // 首頁戰情室資料源 — Dev 用 Mock (不依賴 SP，可獨立驗證前端)；
    // Staging/Production 走真實 usp_Rpt_Home_YtdTarget，正式環境永不顯示假資料。
    if (builder.Environment.IsDevelopment())
        builder.Services.AddScoped<IHomeDashboardRepository, MockHomeDashboardRepository>();
    else
        builder.Services.AddScoped<IHomeDashboardRepository, SqlHomeDashboardRepository>();

    // BLL — 商業邏輯層
    builder.Services.AddScoped<SqlReportService>();
    builder.Services.AddScoped<IReportService>(sp => sp.GetRequiredService<SqlReportService>());
    builder.Services.AddScoped<ICatalogService, CatalogService>();
    builder.Services.AddScoped<IPermissionService, PermissionService>();
    builder.Services.AddScoped<IUsageService, UsageService>();
    builder.Services.AddScoped<IHomeDashboardService, HomeDashboardService>();

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

    app.UseHttpsRedirection();

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
    app.UseAuthentication();

    // 開發環境：未登入時注入 MockWindowsAuth 模擬身分 (清空 UserName 即可測試真正的登入頁)
    if (app.Environment.IsDevelopment())
    {
        app.UseMiddleware<DevWindowsAuthMiddleware>();
    }

    app.UseAuthorization();
    app.UseMiddleware<UnhandledExceptionLoggingMiddleware>();

    // 靜態檔不需登入：登入頁要用到 logo、favicon
    app.MapStaticAssets().AllowAnonymous();
    app.MapRazorPages()
       .WithStaticAssets();

    // ── 非開發環境：驗證必要設定 ─────────────────────────────
    if (!app.Environment.IsDevelopment())
    {
        var cs = app.Configuration.GetSection("ConnectionStrings");
        if (string.IsNullOrWhiteSpace(cs["ReportCenter"]))
            throw new InvalidOperationException("缺少 ConnectionStrings:ReportCenter 設定，請檢查外部機密設定檔。");
        if (string.IsNullOrWhiteSpace(cs["PKSYS"]))
            throw new InvalidOperationException("缺少 ConnectionStrings:PKSYS 設定，請檢查外部機密設定檔。");
        if (string.IsNullOrWhiteSpace(app.Configuration["DataProtection:KeysPath"]))
            throw new InvalidOperationException("缺少 DataProtection:KeysPath 設定，應用程式集區回收後所有人都會被登出。");
    }

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

// ── Helper ───────────────────────────────────────────────────

/// <summary>載入外部 JSON 設定檔，檔案不存在時拋出明確例外。</summary>
static void LoadExternalConfig(ConfigurationManager config, string envVarName, string label)
{
    var path = Environment.GetEnvironmentVariable(envVarName);
    if (string.IsNullOrEmpty(path)) return;

    if (!File.Exists(path))
        throw new FileNotFoundException($"{label}不存在: {path} (環境變數 {envVarName})");

    config.AddJsonFile(path, optional: false, reloadOnChange: false);
}

/// <summary>
/// 是否為瀏覽器的一般換頁 (網址列、連結、表單送出)。fetch / sendBeacon 的 Sec-Fetch-Mode 不是 navigate；
/// 不支援 Sec-Fetch-Mode 的舊瀏覽器以 Accept 是否要求 HTML 判斷。
/// </summary>
static bool IsPageNavigation(HttpRequest request)
{
    var mode = request.Headers["Sec-Fetch-Mode"].ToString();
    return mode.Length > 0
        ? mode == "navigate"
        : request.Headers.Accept.ToString().Contains("text/html", StringComparison.OrdinalIgnoreCase);
}
