using Microsoft.AspNetCore.Mvc;
using ReportCenter.Web.Models;
using ReportCenter.Web.Services;

namespace ReportCenter.Web.Pages.Admin;

/// <summary>
/// 使用分析頁 — Tab1 分析總覽 (KPI、趨勢、熱門/冷門)、Tab2 明細查詢 (篩選 + 伺服器端分頁)。
/// 統計邏輯委派 IUsageService，此 PageModel 僅組裝 View 資料。
/// 存取控制由 AdminPageModel 統一把關。
/// </summary>
public class UsageModel : AdminPageModel
{
    private readonly IUsageService _usageSvc;
    private readonly ICatalogService _catalogSvc;

    public UsageModel(IUsageService usageSvc, ICatalogService catalogSvc,
        IPermissionService permSvc, IReportService reportSvc)
        : base(reportSvc, permSvc)
    {
        _usageSvc = usageSvc;
        _catalogSvc = catalogSvc;
    }

    // ─── 頁面資料屬性 ───

    /// <summary>目前顯示的 Tab (overview / logs)</summary>
    public string Tab { get; set; } = "overview";

    /// <summary>分析總覽 (KPI、30 日趨勢、熱門 Top、冷門清單)</summary>
    public UsageOverview Overview { get; set; } = new();

    /// <summary>明細查詢結果 (分頁)</summary>
    public UsageLogPage Logs { get; set; } = new();

    /// <summary>明細篩選：報表下拉選單 (全部報表，含停用)</summary>
    public List<ReportCatalogItem> AllReports { get; set; } = [];

    // ─── 明細篩選條件 (自 query string 綁定，View 回填表單用) ───

    public int? FilterReportId { get; set; }
    public string FilterUser { get; set; } = "";
    public DateTime? FilterFrom { get; set; }
    public DateTime? FilterTo { get; set; }

    // ─── GET Handler ───

    public IActionResult OnGet(string? tab, int? reportId, string? user,
        DateTime? from, DateTime? to, int p = 1)
    {
        // 帶 reportId 深連結 (如目錄管理下鑽) 一律進明細 Tab
        Tab = tab == "logs" || reportId.HasValue ? "logs" : "overview";

        FilterReportId = reportId;
        FilterUser = user?.Trim() ?? "";
        FilterFrom = from;
        FilterTo = to;

        Overview = _usageSvc.GetOverview();
        AllReports = _catalogSvc.GetCatalogItems();
        Logs = _usageSvc.GetLogs(new UsageLogQuery
        {
            ReportId = reportId,
            UserKeyword = FilterUser,
            From = from,
            To = to,
            Page = p
        });

        return Page();
    }

    // ─── View 輔助 ───

    /// <summary>組出保留目前篩選條件的明細分頁連結</summary>
    public string LogsPageUrl(int page)
    {
        var qs = new List<string> { "tab=logs", $"p={page}" };
        if (FilterReportId.HasValue) qs.Add($"reportId={FilterReportId}");
        if (!string.IsNullOrEmpty(FilterUser)) qs.Add($"user={Uri.EscapeDataString(FilterUser)}");
        if (FilterFrom.HasValue) qs.Add($"from={FilterFrom:yyyy-MM-dd}");
        if (FilterTo.HasValue) qs.Add($"to={FilterTo:yyyy-MM-dd}");
        return "/Admin/Usage?" + string.Join("&", qs);
    }
}
