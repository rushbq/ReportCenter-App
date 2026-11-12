using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ReportCenter.Web.Pages.Account;

/// <summary>
/// 登出本站 (清除 Cookie)。撤銷 DataHubApi 共用的 Refresh Token 由前端 rcLogout() 接著呼叫。
/// 允許匿名：Cookie 已過期的使用者按登出也要成功。
/// </summary>
[AllowAnonymous]
public class LogoutModel : PageModel
{
    private readonly ILogger<LogoutModel> _logger;

    public LogoutModel(ILogger<LogoutModel> logger) => _logger = logger;

    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnPostAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
            _logger.LogInformation("使用者 {Account} 登出報表中心", User.Identity.Name);

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return new JsonResult(new { success = true });
    }
}
