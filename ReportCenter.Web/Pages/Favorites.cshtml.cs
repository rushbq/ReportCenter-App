using Microsoft.AspNetCore.Mvc.RazorPages;
using ReportCenter.Web.Models;
using ReportCenter.Web.Services;

namespace ReportCenter.Web.Pages;

/// <summary>
/// 我的收藏 — 以主畫面卡片呈現使用者收藏的報表，依部門分組，可搜尋、篩選部門與取消收藏。
/// </summary>
public class FavoritesModel : PageModel
{
    private readonly IReportService _svc;

    public FavoritesModel(IReportService svc) => _svc = svc;

    /// <summary>收藏報表 (已依部門 → 自訂排序 → 報表名稱排好)</summary>
    public List<FavoriteReport> Favorites { get; set; } = [];

    /// <summary>有收藏的部門 (依側欄順序)，供部門篩選與分組標題使用</summary>
    public List<FavoriteDeptGroup> Groups { get; set; } = [];

    public void OnGet()
    {
        Favorites = _svc.GetFavoriteReports();

        var deptIcons = _svc.GetDepartments().ToDictionary(d => d.Id, d => d.Icon);
        Groups = Favorites
            .GroupBy(f => (f.DeptId, f.DeptLabel))
            .Select(g => new FavoriteDeptGroup
            {
                DeptId = g.Key.DeptId,
                Label = g.Key.DeptLabel,
                Icon = deptIcons.GetValueOrDefault(g.Key.DeptId, "folder"),
            })
            .ToList();
    }
}
