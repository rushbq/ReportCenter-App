namespace ReportCenter.Web.Services;

using ReportCenter.Web.Models;
using ReportCenter.Web.Models.Enums;
using ReportCenter.Web.Models.Settings;

/// <summary>
/// 報表目錄管理商業邏輯介面 (BLL)
/// </summary>
public interface ICatalogService
{
    // ─── 報表目錄 ───
    List<ReportCatalogItem> GetCatalogItems(string? toolFilter = null, bool? isActive = null, string? search = null);
    ReportCatalogItem? GetCatalogItem(int reportId);
    ReportCatalogItem SaveCatalogItem(ReportCatalogItem item);
    bool DeleteCatalogItem(int reportId);
    bool ToggleCatalogItem(int reportId);
    AdminStats GetAdminStats();

    // ─── 相依物件 ───
    List<string> GetAllDependencyObjects();

    // ─── 報表分類 ───
    List<ReportCategory> GetCategories();
    ReportCategory? GetCategory(int categoryId);
    ReportCategory SaveCategory(ReportCategory category);
    bool DeleteCategory(int categoryId);

    // ─── 部門指派 (依公司別過濾) ───
    List<CatalogDept> GetCatalogDepartments(string companyId);

    // ─── 報表資料夾列舉 ───
    IReadOnlyList<(ReportFolder Value, string Label)> GetReportFolders();

    // ─── BaseUrl 設定 ───
    ReportBaseUrlSettings GetBaseUrls();
}
