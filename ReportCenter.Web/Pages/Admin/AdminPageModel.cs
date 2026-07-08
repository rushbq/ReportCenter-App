using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReportCenter.Web.Services;

namespace ReportCenter.Web.Pages.Admin;

/// <summary>
/// 系統管理頁基底 — 對「所有」page handler (GET/POST/AJAX) 強制檢查 AdminUsers 白名單。
/// 避免僅在 OnGet 檢查、卻讓 POST 寫入端點裸露而造成越權 (Broken Access Control)。
/// 非管理員：GET 導向 AccessDenied 頁面；POST/寫入端點回傳 403 JSON。
/// </summary>
public abstract class AdminPageModel : PageModel
{
    private readonly IReportService _reportSvc;
    private readonly IPermissionService _permSvc;

    protected AdminPageModel(IReportService reportSvc, IPermissionService permSvc)
    {
        _reportSvc = reportSvc;
        _permSvc = permSvc;
    }

    /// <summary>是否無管理員權限 (View 用於切換顯示 _AccessDenied)</summary>
    public bool AccessDenied { get; private set; }

    /// <summary>目前登入者的員工編號 (供子類別記錄 GrantedBy 等用途)</summary>
    protected string CurrentUserId => _reportSvc.GetCurrentUser().Id;

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (_permSvc.IsAdmin(CurrentUserId)) return;

        AccessDenied = true;
        context.Result = HttpMethods.IsPost(HttpContext.Request.Method)
            ? new JsonResult(new { success = false, message = "權限不足" })
            {
                StatusCode = StatusCodes.Status403Forbidden
            }
            : Page();
    }
}
