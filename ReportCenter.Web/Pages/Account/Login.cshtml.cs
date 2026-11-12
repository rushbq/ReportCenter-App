using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using ReportCenter.Web.Models.Settings;
using ReportCenter.Web.Services;

namespace ReportCenter.Web.Pages.Account;

/// <summary>
/// 登入頁 — 先以 DataHubApi 的 Refresh Token 自動登入 (在 PKPortal 等站台登入過即可免登)，
/// 失敗才顯示帳密表單。帳密直接送 DataHubApi，不經過本站伺服器；
/// 本站只負責驗證 DataHubApi 發出的 JWT 並發本站 Cookie。
/// </summary>
[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly IDataHubTokenValidator _validator;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(
        IDataHubTokenValidator validator,
        IOptions<DataHubAuthSettings> authSettings,
        IOptions<SiteSettings> siteSettings,
        ILogger<LoginModel> logger)
    {
        _validator = validator;
        _logger = logger;
        ApiBaseUrl = authSettings.Value.ApiBaseUrl;
        SiteTitle = siteSettings.Value.Title;
    }

    /// <summary>DataHubApi 的 API 根網址 (前端呼叫 Login / Refresh 用)</summary>
    public string ApiBaseUrl { get; }

    /// <summary>站台名稱</summary>
    public string SiteTitle { get; }

    /// <summary>登入後要導回的站內網址</summary>
    public string ReturnUrl { get; private set; } = "/";

    /// <summary>是否剛登出 (剛登出時不做自動登入)</summary>
    public bool LoggedOut { get; private set; }

    public IActionResult OnGet(string? returnUrl, bool loggedOut = false)
    {
        ReturnUrl = SafeReturnUrl(returnUrl);
        LoggedOut = loggedOut;

        if (!loggedOut && User.Identity?.IsAuthenticated == true)
            return LocalRedirect(ReturnUrl);

        return Page();
    }

    /// <summary>以 DataHubApi 發出的 JWT 登入本站</summary>
    public async Task<IActionResult> OnPostSignInAsync([FromForm] string? token, [FromForm] string? returnUrl)
    {
        var account = await _validator.ValidateAsync(token);
        if (account == null)
        {
            return new JsonResult(new { success = false, message = "登入憑證無效，請重新登入" })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }

        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, account)],
            CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });

        _logger.LogInformation("使用者 {Account} 登入報表中心", account);
        return new JsonResult(new { success = true, redirect = SafeReturnUrl(returnUrl) });
    }

    /// <summary>只接受站內網址，避免被當成開放轉址</summary>
    private string SafeReturnUrl(string? url)
        => !string.IsNullOrEmpty(url) && Url.IsLocalUrl(url) && !url.StartsWith("/Account/", StringComparison.OrdinalIgnoreCase)
            ? url
            : "/";
}
