using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using ReportCenter.Web.Models;
using ReportCenter.Web.Models.Settings;
using ReportCenter.Web.Services;

namespace ReportCenter.Web.Pages.Debug;

/// <summary>
/// Windows 驗證診斷頁 — 顯示目前 HttpContext.User 與 SqlReportService 的解析結果，供 IIS 正式環境除錯。
/// 預設關閉，需於設定檔啟用 WindowsAuthDebug:Enabled。
/// </summary>
public class WindowsAuthModel : PageModel
{
    private readonly WindowsAuthDebugSettings _settings;
    private readonly IWebHostEnvironment _environment;
    private readonly SqlReportService _reportService;

    public WindowsAuthModel(
        IOptions<WindowsAuthDebugSettings> settings,
        IWebHostEnvironment environment,
        SqlReportService reportService)
    {
        _settings = settings.Value;
        _environment = environment;
        _reportService = reportService;
    }

    /// <summary>Windows 驗證 principal 與 SqlReportService 解析後的資訊</summary>
    public WindowsAuthDebugInfo AuthInfo { get; private set; } = new();

    /// <summary>目前請求 TraceId</summary>
    public string TraceId { get; private set; } = "";

    /// <summary>請求來源 IP</summary>
    public string RemoteIp { get; private set; } = "";

    /// <summary>反向代理轉送 IP</summary>
    public string ForwardedFor { get; private set; } = "";

    /// <summary>目前 User-Agent</summary>
    public string UserAgent { get; private set; } = "";

    /// <summary>目前請求完整路徑</summary>
    public string RequestPath { get; private set; } = "";

    /// <summary>目前請求 Scheme</summary>
    public string RequestScheme { get; private set; } = "";

    /// <summary>目前請求 Host</summary>
    public string RequestHost { get; private set; } = "";

    /// <summary>目前環境名稱</summary>
    public string EnvironmentName { get; private set; } = "";

    /// <summary>伺服器機器名稱</summary>
    public string MachineName { get; private set; } = "";

    /// <summary>伺服器當下時間</summary>
    public DateTime ServerTime { get; private set; }

    public IActionResult OnGet()
    {
        if (!_settings.Enabled)
            return NotFound();

        if (!_environment.IsDevelopment() && User.Identity?.IsAuthenticated != true)
            return Challenge();

        AuthInfo = _reportService.GetCurrentUserDebugInfo();
        TraceId = HttpContext.TraceIdentifier;
        RemoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        ForwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].ToString();
        UserAgent = HttpContext.Request.Headers.UserAgent.ToString();
        RequestPath = $"{HttpContext.Request.PathBase}{HttpContext.Request.Path}{HttpContext.Request.QueryString}";
        RequestScheme = HttpContext.Request.Scheme;
        RequestHost = HttpContext.Request.Host.Value ?? "";
        EnvironmentName = _environment.EnvironmentName;
        MachineName = Environment.MachineName;
        ServerTime = DateTime.Now;

        return Page();
    }
}
