namespace ReportCenter.Web.Services;

using System.Security.Claims;
using ReportCenter.Web.Models;

/// <summary>
/// SQL Server 實作 — 儀表板/報表明細仍沿用 Mock 資料
/// 目錄管理已移至 ICatalogService / ICatalogRepository
/// </summary>
public class SqlReportService : IReportService
{
    private readonly MockReportService _mock = new();
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SqlReportService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 從 HttpContext.User (Windows 驗證 / 開發模擬) 取得目前使用者資訊。
    /// 未來可透過 AD/DB 查詢補充使用者詳細資料。
    /// </summary>
    public UserInfo GetCurrentUser()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
            return _mock.GetCurrentUser();

        var accountName = principal.Identity.Name ?? "";
        var displayName = principal.FindFirst("DisplayName")?.Value ?? "";
        var empId = principal.FindFirst("EmployeeId")?.Value ?? "";
        var deptName = principal.FindFirst("Department")?.Value ?? "";
        var deptId = principal.FindFirst("DepartmentId")?.Value ?? "";

        if (string.IsNullOrEmpty(displayName))
            displayName = ExtractUserName(accountName);
        if (string.IsNullOrEmpty(empId))
            empId = accountName;

        return new UserInfo
        {
            Id = empId,
            Name = displayName,
            DeptId = deptId,
            DeptName = deptName,
        };
    }

    /// <summary>從 DOMAIN\username 格式取出 username</summary>
    private static string ExtractUserName(string accountName)
    {
        var idx = accountName.IndexOf('\\');
        return idx >= 0 ? accountName[(idx + 1)..] : accountName;
    }

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
