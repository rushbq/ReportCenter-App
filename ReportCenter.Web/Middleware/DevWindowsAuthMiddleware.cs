using System.Security.Claims;
using Microsoft.Extensions.Options;
using ReportCenter.Web.Models.Settings;

namespace ReportCenter.Web.Middleware;

/// <summary>
/// 開發環境專用：尚未登入時，將 appsettings.Development.json 中 MockWindowsAuth 區段的設定
/// 注入為 ClaimsPrincipal，本機不必連 DataHubApi 也能以指定身分操作。
/// 已透過登入頁取得 Cookie 時不覆蓋；清空 UserName 即可在本機測試真正的登入流程。
/// （類別與設定名稱沿用舊的 Windows 驗證命名）
/// </summary>
public class DevWindowsAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly MockWindowsAuthSettings _settings;

    public DevWindowsAuthMiddleware(RequestDelegate next, IOptions<MockWindowsAuthSettings> settings)
    {
        _next = next;
        _settings = settings.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!string.IsNullOrEmpty(_settings.UserName) && context.User.Identity?.IsAuthenticated != true)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, _settings.UserName),
                new("DisplayName", _settings.DisplayName),
                new("EmployeeId", _settings.EmployeeId),
                new("Department", _settings.Department),
                new("DepartmentId", _settings.DepartmentId),
            };

            var identity = new ClaimsIdentity(claims, "DevMock");
            context.User = new ClaimsPrincipal(identity);
        }

        await _next(context);
    }
}
