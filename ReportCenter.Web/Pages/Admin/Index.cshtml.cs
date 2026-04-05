using Microsoft.AspNetCore.Mvc.RazorPages;
using ReportCenter.Web.Models;
using ReportCenter.Web.Services;

namespace ReportCenter.Web.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly ICatalogService _catalogSvc;
    private readonly IReportService _reportSvc;

    public IndexModel(ICatalogService catalogSvc, IReportService reportSvc)
    {
        _catalogSvc = catalogSvc;
        _reportSvc = reportSvc;
    }

    public AdminStats Stats { get; set; } = null!;
    public List<Department> Departments { get; set; } = [];

    public void OnGet()
    {
        Stats = _catalogSvc.GetAdminStats();
        Departments = _reportSvc.GetDepartments();
    }
}
