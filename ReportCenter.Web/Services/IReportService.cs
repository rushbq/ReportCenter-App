namespace ReportCenter.Web.Services;

using ReportCenter.Web.Models;

/// <summary>
/// 報表資料服務介面 — 實作此介面即可切換為真實 API 資料來源
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

    // ─── 報表目錄管理 ───
    List<ReportCatalogItem> GetCatalogItems(string? toolFilter = null, bool? isActive = null, string? search = null);
    ReportCatalogItem? GetCatalogItem(int reportId);
    ReportCatalogItem SaveCatalogItem(ReportCatalogItem item);
    bool DeleteCatalogItem(int reportId);
    AdminStats GetAdminStats();
    List<string> GetAllDependencyObjects();

    // ─── 報表分類管理 ───
    List<ReportCategory> GetCategories(int? deptId = null);
    ReportCategory? GetCategory(int categoryId);
    ReportCategory SaveCategory(ReportCategory category);
    bool DeleteCategory(int categoryId);

    // ─── 部門 (User_Dept) ───
    List<CatalogDept> GetCatalogDepartments(string? area = null);
}
