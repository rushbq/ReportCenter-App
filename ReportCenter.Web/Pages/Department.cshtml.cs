using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using ReportCenter.Web.Models;
using ReportCenter.Web.Models.Settings;
using ReportCenter.Web.Services;

namespace ReportCenter.Web.Pages;

public class DepartmentModel : PageModel
{
    private readonly IReportService _svc;
    private readonly ReportBaseUrlSettings _baseUrls;

    public DepartmentModel(IReportService svc, IOptions<ReportBaseUrlSettings> baseUrls)
    {
        _svc = svc;
        _baseUrls = baseUrls.Value;
    }

    public string DeptId { get; set; } = "";
    public Department Dept { get; set; } = null!;
    public List<Report> Reports { get; set; } = [];
    public List<int> FavoriteIds { get; set; } = [];
    public string SmartQueryBaseUrl => _baseUrls.SmartQuery;
    public string SsrsBaseUrl => _baseUrls.SSRS;

    public void OnGet(string dept)
    {
        DeptId = dept ?? "";
        var departments = _svc.GetDepartments();
        Dept = _svc.GetDepartment(DeptId) ?? (departments.Count > 0 ? departments[0] : new Department { Label = "未知部門" });
        if (string.IsNullOrEmpty(DeptId) && departments.Count > 0)
            DeptId = Dept.Id;
        Reports = _svc.GetReports(DeptId);
        FavoriteIds = _svc.GetUserFavorites();
    }
}
