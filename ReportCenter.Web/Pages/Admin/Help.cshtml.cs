using Microsoft.AspNetCore.Mvc;
using ReportCenter.Web.Services;

namespace ReportCenter.Web.Pages.Admin;

/// <summary>
/// 管理者使用手冊 — 以操作步驟與截圖說明系統管理功能，內容為靜態文字。
/// 僅限 AdminUsers 白名單中的使用者存取 (由 AdminPageModel 統一把關)。
/// </summary>
public class HelpModel : AdminPageModel
{
    public HelpModel(IReportService reportSvc, IPermissionService permSvc)
        : base(reportSvc, permSvc)
    {
    }

    public IActionResult OnGet() => Page();
}
