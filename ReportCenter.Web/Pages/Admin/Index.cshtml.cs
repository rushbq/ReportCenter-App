using Microsoft.AspNetCore.Mvc;
using ReportCenter.Web.Models;
using ReportCenter.Web.Services;

namespace ReportCenter.Web.Pages.Admin;

/// <summary>
/// 管理總覽頁 — 顯示報表清冊 KPI、部門使用矩陣、孤立報表、冷門報表。
/// 所有統計邏輯委派給 ICatalogService / IUsageService，此 PageModel 僅負責組裝 View 所需的資料。
/// 僅限 AdminUsers 白名單中的使用者存取 (由 AdminPageModel 統一把關)。
/// </summary>
public class IndexModel : AdminPageModel
{
    private readonly ICatalogService _catalogSvc;
    private readonly IUsageService _usageSvc;

    public IndexModel(ICatalogService catalogSvc, IUsageService usageSvc,
        IPermissionService permSvc, IReportService reportSvc)
        : base(reportSvc, permSvc)
    {
        _catalogSvc = catalogSvc;
        _usageSvc = usageSvc;
    }

    /// <summary>報表清冊統計 (KPI、部門矩陣、孤立報表)</summary>
    public AdminStats Stats { get; set; } = null!;

    /// <summary>總覽頁冷門報表的預覽筆數上限 (完整清單在 /Admin/Usage)</summary>
    private const int ColdPreviewLimit = 6;

    /// <summary>冷門報表預覽 (啟用中且 90 日內 0 次使用)，最多 ColdPreviewLimit 筆</summary>
    public List<ReportUsageSummary> ColdReports { get; set; } = [];

    /// <summary>冷門報表總數 (未受預覽筆數限制)</summary>
    public int ColdTotal { get; set; }

    public IActionResult OnGet()
    {
        Stats = _catalogSvc.GetAdminStats();

        // 警示區塊只需摘要：Service 已依最後使用時間排序 (從未使用者在前)，取前幾筆即最需處理的
        var cold = _usageSvc.GetColdReports();
        ColdTotal = cold.Count;
        ColdReports = cold.Take(ColdPreviewLimit).ToList();

        return Page();
    }
}
