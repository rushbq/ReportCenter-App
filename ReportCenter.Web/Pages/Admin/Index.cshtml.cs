using Microsoft.AspNetCore.Mvc.RazorPages;
using ReportCenter.Web.Models;
using ReportCenter.Web.Services;

namespace ReportCenter.Web.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly IReportService _svc;

    public IndexModel(IReportService svc) => _svc = svc;

    public AdminStats Stats { get; set; } = null!;
    public List<Department> Departments { get; set; } = [];

    public void OnGet()
    {
        Stats = _svc.GetAdminStats();
        Departments = _svc.GetDepartments();
    }
}
