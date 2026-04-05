using System.Security.Claims;
using Microsoft.Extensions.Options;
using ReportCenter.Web.Models.Settings;

namespace ReportCenter.Web.Middleware;

/// <summary>
/// 開發環境專用：模擬 Windows AD 驗證，將 appsettings.Development.json 中
/// MockWindowsAuth 區段的設定注入為 ClaimsPrincipal，使 macOS 開發環境
/// 也能擁有與 IIS Windows 驗證相同的 HttpContext.User 結構。
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
        if (!string.IsNullOrEmpty(_settings.UserName))
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, _settings.UserName),
                new("DisplayName", _settings.DisplayName),
                new("EmployeeId", _settings.EmployeeId),
                new("Department", _settings.Department),
                new("DepartmentId", _settings.DepartmentId),
            };

            // 使用 "Negotiate" 作為 AuthenticationType，模擬 Windows 驗證行為
            var identity = new ClaimsIdentity(claims, "Negotiate");
            context.User = new ClaimsPrincipal(identity);
        }

        await _next(context);
    }
}
