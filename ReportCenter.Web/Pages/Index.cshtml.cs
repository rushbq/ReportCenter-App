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

    public string ActiveCompanyId { get; set; } = "";

    public void OnGet()
    {
        CurrentUser = _svc.GetCurrentUser();
        Companies = _svc.GetCompanies();

        var cookieCompanyId = HttpContext.Request.Cookies["companyId"];
        ActiveCompanyId = !string.IsNullOrEmpty(cookieCompanyId) && Companies.Any(c => c.Id == cookieCompanyId)
            ? cookieCompanyId
            : CurrentUser.CompanyId;

        Kpi = _svc.GetDashboardKpi(ActiveCompanyId);
        QuickAccessItems = _svc.GetQuickAccessItems();
        RevenueChart = _svc.GetRevenueChartData(ActiveCompanyId);
        DeptChart = _svc.GetDeptComparisonData(ActiveCompanyId);
    }
}
