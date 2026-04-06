namespace ReportCenter.Web.Services;

using ReportCenter.Web.Models;
using ReportCenter.Web.Models.Enums;
using ReportCenter.Web.Models.Settings;

/// <summary>
/// 報表目錄管理商業邏輯介面 (BLL)。
/// 定義報表 CRUD、統計、分類、部門指派等操作契約。
/// </summary>
public interface ICatalogService
{
    // ─── 報表目錄 CRUD ───

    /// <summary>取得報表清冊清單，可依工具類型、啟停用狀態、搜尋關鍵字篩選</summary>
    List<ReportCatalogItem> GetCatalogItems(string? toolFilter = null, bool? isActive = null, string? search = null);

    /// <summary>取得單一報表目錄項目</summary>
    ReportCatalogItem? GetCatalogItem(int reportId);

    /// <summary>新增或更新報表目錄項目 (含部門指派與相依物件)</summary>
    ReportCatalogItem SaveCatalogItem(ReportCatalogItem item);

    /// <summary>刪除報表目錄項目</summary>
    bool DeleteCatalogItem(int reportId);

    /// <summary>切換報表啟停用狀態</summary>
    bool ToggleCatalogItem(int reportId);

    /// <summary>
    /// 取得管理總覽統計資料 (KPI、部門使用矩陣、孤立報表、最近異動)。
    /// 部門矩陣僅納入 appsettings DepartmentDisplay 中設定的部門。
    /// </summary>
    AdminStats GetAdminStats();

    // ─── 相依物件 ───

    /// <summary>取得所有已知的相依物件名稱 (供 autocomplete)</summary>
    List<string> GetAllDependencyObjects();

    // ─── 報表分類 ───

    /// <summary>取得所有報表分類</summary>
    List<ReportCategory> GetCategories();

    /// <summary>取得單一報表分類</summary>
    ReportCategory? GetCategory(int categoryId);

    /// <summary>新增或更新報表分類</summary>
    ReportCategory SaveCategory(ReportCategory category);

    /// <summary>刪除報表分類</summary>
    bool DeleteCategory(int categoryId);

    // ─── 部門指派 ───

    /// <summary>
    /// 取得指定公司別的部門清單 (從 User_Dept 依 Area 過濾)。
    /// 僅回傳 appsettings DepartmentDisplay 中設定的部門，確保與側邊欄一致。
    /// 用於報表編輯 Modal 的部門指派 checkbox。
    /// </summary>
    List<CatalogDept> GetCatalogDepartments(string companyId);

    // ─── 報表資料夾列舉 ───

    /// <summary>取得報表資料夾列舉 (對應 ReportFolder enum)</summary>
    IReadOnlyList<(ReportFolder Value, string Label)> GetReportFolders();

    // ─── BaseUrl 設定 ───

    /// <summary>取得外部報表 BaseUrl 設定 (SmartQuery / SSRS)</summary>
    ReportBaseUrlSettings GetBaseUrls();
}
