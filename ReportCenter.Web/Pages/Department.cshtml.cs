using Microsoft.AspNetCore.Mvc;
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

    public IActionResult OnGet(string dept)
    {
        DeptId = dept ?? "";
        var departments = _svc.GetDepartments();
        var matched = _svc.GetDepartment(DeptId);

        // 網址上的 dept 不屬於目前公司時（例如切換公司後重新載入），導向目前公司的第一個部門，
        // 避免標題與報表清單分屬不同部門
        if (matched == null)
        {
            if (departments.Count > 0)
                return RedirectToPage("/Department", new { dept = departments[0].Id });

            Dept = new Department { Label = "未知部門" };
            return Page();
        }

        Dept = matched;
        Reports = _svc.GetReports(DeptId);
        FavoriteIds = _svc.GetUserFavorites();
        return Page();
    }
}
