using Microsoft.AspNetCore.Mvc.RazorPages;
using ReportCenter.Web.Models;
using ReportCenter.Web.Services;

namespace ReportCenter.Web.Pages;

public class DepartmentModel : PageModel
{
    private readonly IReportService _svc;

    public DepartmentModel(IReportService svc) => _svc = svc;

    public string DeptId { get; set; } = "procurement";
    public Department Dept { get; set; } = null!;
    public List<Report> Reports { get; set; } = [];

    public void OnGet(string dept)
    {
        DeptId = dept ?? "procurement";
        Dept = _svc.GetDepartment(DeptId) ?? _svc.GetDepartments()[0];
        Reports = _svc.GetReports(DeptId);
    }
}
