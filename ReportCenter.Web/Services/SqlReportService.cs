namespace ReportCenter.Web.Services;

using ReportCenter.Web.Models;

/// <summary>
/// SQL Server 實作 — 儀表板/報表明細仍沿用 Mock 資料
/// 目錄管理已移至 ICatalogService / ICatalogRepository
/// </summary>
public class SqlReportService : IReportService
{
    private readonly MockReportService _mock = new();

    public UserInfo GetCurrentUser() => _mock.GetCurrentUser();
    public List<Company> GetCompanies() => _mock.GetCompanies();
    public List<Department> GetDepartments() => _mock.GetDepartments();
    public Department? GetDepartment(string deptId) => _mock.GetDepartment(deptId);
    public List<Report> GetReports(string deptId) => _mock.GetReports(deptId);
    public Report? GetReport(string deptId, string reportName) => _mock.GetReport(deptId, reportName);
    public List<QuickAccess> GetQuickAccessItems() => _mock.GetQuickAccessItems();
    public List<MaterialRow> GetMaterialRows(string deptId, string reportName, int page = 1, int pageSize = 20)
        => _mock.GetMaterialRows(deptId, reportName, page, pageSize);
    public int GetMaterialRowCount(string deptId, string reportName) => _mock.GetMaterialRowCount(deptId, reportName);
    public DashboardKpi GetDashboardKpi(string companyId) => _mock.GetDashboardKpi(companyId);
    public ChartData GetRevenueChartData(string companyId, string period = "month") => _mock.GetRevenueChartData(companyId, period);
    public ChartData GetDeptComparisonData(string companyId) => _mock.GetDeptComparisonData(companyId);
    public ChartData GetReportChartData(string deptId, string reportName, string chartType)
        => _mock.GetReportChartData(deptId, reportName, chartType);
}
