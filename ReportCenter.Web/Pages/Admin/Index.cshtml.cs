using Microsoft.AspNetCore.Mvc.RazorPages;
using ReportCenter.Web.Models;
using ReportCenter.Web.Services;

namespace ReportCenter.Web.Pages.Admin;

/// <summary>
/// 管理總覽頁 — 顯示報表清冊 KPI、部門使用矩陣、孤立報表、相依性分析。
/// 所有統計邏輯委派給 ICatalogService，此 PageModel 僅負責組裝 View 所需的資料。
/// </summary>
public class IndexModel : PageModel
{
    private readonly ICatalogService _catalogSvc;

    public IndexModel(ICatalogService catalogSvc)
    {
        _catalogSvc = catalogSvc;
    }

    /// <summary>報表清冊統計 (KPI、部門矩陣、孤立報表、最近異動)</summary>
    public AdminStats Stats { get; set; } = null!;

    /// <summary>相依性分析資料 (依物件分組，列出引用的報表名稱)</summary>
    public List<DependencyGroup> DepAnalysis { get; set; } = [];

    public void OnGet()
    {
        Stats = _catalogSvc.GetAdminStats();
        DepAnalysis = BuildDependencyAnalysis();
    }

    /// <summary>
    /// 從最近異動 + 孤立報表中蒐集所有相依物件，
    /// 以物件名稱分組並列出引用的報表清單，依引用數降冪排序。
    /// </summary>
    private List<DependencyGroup> BuildDependencyAnalysis()
    {
        var allItems = Stats.RecentItems
            .Concat(Stats.OrphanItems)
            .DistinctBy(r => r.ReportID);

        return allItems
            .SelectMany(r => r.Dependencies.Select(dep => new { dep, r.ReportName }))
            .GroupBy(x => x.dep)
            .Select(g => new DependencyGroup
            {
                Name = g.Key,
                Reports = g.Select(x => x.ReportName).Distinct().ToList()
            })
            .OrderByDescending(g => g.Reports.Count)
            .ToList();
    }
}

/// <summary>相依性分析 ViewModel — 一個物件被哪些報表引用</summary>
public class DependencyGroup
{
    public string Name { get; set; } = "";
    public List<string> Reports { get; set; } = [];
}
