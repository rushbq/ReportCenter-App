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

    /// <summary>目前公司別：companyId cookie 為合法公司才採用，否則退回使用者預設公司</summary>
    string GetCurrentCompanyId();

    /// <summary>組出外部報表完整連結 (BaseUrl + ReportCode，需要時附加 dbs 公司別參數)</summary>
    string BuildReportUrl(Report report);

    // 部門
    List<Department> GetDepartments();
    Department? GetDepartment(string deptId);

    // 報表
    List<Report> GetReports(string deptId);
    Report? GetReport(string deptId, string reportName);

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

    /// <summary>切換收藏；回傳 false 代表報表不存在或使用者無權限 (未變更任何資料)</summary>
    bool ToggleFavorite(int reportId);

    // 釘選
    List<Report> GetUserPins();

    /// <summary>切換釘選；回傳 false 代表報表不存在或使用者無權限 (未變更任何資料)</summary>
    bool TogglePin(int reportId);
}
