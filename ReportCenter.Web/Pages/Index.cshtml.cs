using Microsoft.AspNetCore.Mvc.RazorPages;
using ReportCenter.Web.Models;
using ReportCenter.Web.Services;

namespace ReportCenter.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IReportService _svc;

    public IndexModel(IReportService svc) => _svc = svc;

    public UserInfo CurrentUser { get; set; } = null!;
    public List<Company> Companies { get; set; } = [];
    public DashboardKpi Kpi { get; set; } = null!;
    public List<QuickAccess> QuickAccessItems { get; set; } = [];
    public ChartData RevenueChart { get; set; } = null!;
    public ChartData DeptChart { get; set; } = null!;

    public void OnGet()
    {
        CurrentUser = _svc.GetCurrentUser();
        Companies = _svc.GetCompanies();
        Kpi = _svc.GetDashboardKpi(CurrentUser.CompanyId);
        QuickAccessItems = _svc.GetQuickAccessItems();
        RevenueChart = _svc.GetRevenueChartData(CurrentUser.CompanyId);
        DeptChart = _svc.GetDeptComparisonData(CurrentUser.CompanyId);
    }
}
