namespace ReportCenter.Web.Services;

using ReportCenter.Web.Models;

/// <summary>
/// 報表資料服務介面 — 負責儀表板、部門、報表明細等核心資料
/// </summary>
public interface IReportService
{
    // 使用者與公司
    UserInfo GetCurrentUser();
    List<Company> GetCompanies();

    // 部門
    List<Department> GetDepartments();
    Department? GetDepartment(string deptId);

    // 報表
    List<Report> GetReports(string deptId);
    Report? GetReport(string deptId, string reportName);

    // 快速存取
    List<QuickAccess> GetQuickAccessItems();

    // 明細資料
    List<MaterialRow> GetMaterialRows(string deptId, string reportName, int page = 1, int pageSize = 20);
    int GetMaterialRowCount(string deptId, string reportName);

    // 儀表板 KPI
    DashboardKpi GetDashboardKpi(string companyId);

    // 圖表資料
    ChartData GetRevenueChartData(string companyId, string period = "month");
    ChartData GetDeptComparisonData(string companyId);
    ChartData GetReportChartData(string deptId, string reportName, string chartType);

    // 收藏
    List<int> GetUserFavorites();
    void ToggleFavorite(int reportId);

    // 釘選
    List<Report> GetUserPins();
    void TogglePin(int reportId);
}
