namespace ReportCenter.Web.Repositories;

using ReportCenter.Web.Models;

/// <summary>
/// 報表目錄資料存取介面 (DAL)
/// </summary>
public interface ICatalogRepository
{
    // ─── 報表目錄 CRUD ───
    List<ReportCatalogItem> GetCatalogItems(string? toolFilter = null, bool? isActive = null, string? search = null);
    ReportCatalogItem? GetCatalogItem(int reportId);
    ReportCatalogItem SaveCatalogItem(ReportCatalogItem item);
    bool DeleteCatalogItem(int reportId);

    // ─── 相依物件 ───
    List<string> GetAllDependencyObjects();

    // ─── 報表分類 ───
    List<ReportCategory> GetCategories();
    ReportCategory? GetCategory(int categoryId);
    ReportCategory SaveCategory(ReportCategory category);
    bool DeleteCategory(int categoryId);

    // ─── 部門 (User_Dept) ───
    List<CatalogDept> GetCatalogDepartments(string? area = null);
}
