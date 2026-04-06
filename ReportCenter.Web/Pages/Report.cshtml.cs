using Microsoft.AspNetCore.Mvc.RazorPages;
using ReportCenter.Web.Models;
using ReportCenter.Web.Services;

namespace ReportCenter.Web.Pages;

public class ReportModel : PageModel
{
    private readonly IReportService _svc;

    public ReportModel(IReportService svc) => _svc = svc;

    public string DeptId { get; set; } = "";
    public string ReportName { get; set; } = "";
    public int ReportID { get; set; }
    public bool IsFavorite { get; set; }
    public Department Dept { get; set; } = null!;
    public List<MaterialRow> MaterialRows { get; set; } = [];
    public int TotalRows { get; set; }
    public int CurrentPage { get; set; } = 1;

    public void OnGet(string dept, string name, int page = 1)
    {
        DeptId = dept ?? "";
        ReportName = name ?? "";
        CurrentPage = page < 1 ? 1 : page;
        var departments = _svc.GetDepartments();
        Dept = _svc.GetDepartment(DeptId) ?? (departments.Count > 0 ? departments[0] : new Department { Label = "未知部門" });
        if (string.IsNullOrEmpty(DeptId) && departments.Count > 0)
            DeptId = Dept.Id;

        // 取得報表 ID 供收藏功能使用
        var report = _svc.GetReport(DeptId, ReportName);
        ReportID = report?.ReportID ?? 0;

        var favorites = _svc.GetUserFavorites();
        IsFavorite = favorites.Contains(ReportID);

        MaterialRows = _svc.GetMaterialRows(DeptId, ReportName, CurrentPage);
        TotalRows = _svc.GetMaterialRowCount(DeptId, ReportName);
    }
}
