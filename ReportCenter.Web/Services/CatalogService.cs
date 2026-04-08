namespace ReportCenter.Web.Services;

using Microsoft.Extensions.Options;
using ReportCenter.Web.Models;
using ReportCenter.Web.Models.Enums;
using ReportCenter.Web.Models.Settings;
using ReportCenter.Web.Repositories;

/// <summary>
/// 報表目錄管理商業邏輯實作 (BLL)。
/// 職責：報表 CRUD、統計儀表板資料、部門指派、分類管理。
/// 依賴 ICatalogRepository 處理資料存取，IReportService 提供公司/區域資訊。
/// </summary>
public class CatalogService : ICatalogService
{
    private readonly ICatalogRepository _repo;
    private readonly IPksysRepository _pksysRepo;
    private readonly IReportService _reportSvc;
    private readonly ReportBaseUrlSettings _baseUrls;
    private readonly DepartmentDisplaySettings _deptDisplay;

    public CatalogService(ICatalogRepository repo, IPksysRepository pksysRepo, IReportService reportSvc,
        IOptions<ReportBaseUrlSettings> baseUrls, IOptions<DepartmentDisplaySettings> deptDisplay)
    {
        _repo = repo;
        _pksysRepo = pksysRepo;
        _reportSvc = reportSvc;
        _baseUrls = baseUrls.Value;
        _deptDisplay = deptDisplay.Value;
    }

    // ─── 報表目錄 CRUD ───

    /// <inheritdoc />
    public List<ReportCatalogItem> GetCatalogItems(string? toolFilter = null, bool? isActive = null, string? search = null)
        => _repo.GetCatalogItems(toolFilter, isActive, search);

    /// <inheritdoc />
    public ReportCatalogItem? GetCatalogItem(int reportId)
        => _repo.GetCatalogItem(reportId);

    /// <inheritdoc />
    public ReportCatalogItem SaveCatalogItem(ReportCatalogItem item)
        => _repo.SaveCatalogItem(item);

    /// <inheritdoc />
    public bool DeleteCatalogItem(int reportId)
        => _repo.DeleteCatalogItem(reportId);

    /// <inheritdoc />
    public bool ToggleCatalogItem(int reportId)
    {
        var item = _repo.GetCatalogItem(reportId);
        if (item == null) return false;
        item.IsActive = !item.IsActive;
        _repo.SaveCatalogItem(item);
        return true;
    }

    // ─── 管理總覽統計 ───

    /// <inheritdoc />
    public AdminStats GetAdminStats()
    {
        var items = _repo.GetCatalogItems();

        var stats = new AdminStats
        {
            TotalReports = items.Count,
            ActiveReports = items.Count(i => i.IsActive),
            InactiveReports = items.Count(i => !i.IsActive),
            InternalCount = items.Count(i => i.ReportTool == "Internal"),
            SmartQueryCount = items.Count(i => i.ReportTool == "SmartQuery"),
            SsrsCount = items.Count(i => i.ReportTool == "SSRS"),
            OrphanCount = items.Count(i => i.Departments.Count == 0),
            RecentItems = items.OrderByDescending(i => i.ModifyDate ?? i.CreateDate).Take(10).ToList(),
            OrphanItems = items.Where(i => i.Departments.Count == 0).ToList(),
        };

        // 依 appsettings DepartmentDisplay 設定，分區建置部門使用矩陣
        var activeItems = items.Where(i => i.IsActive).ToList();
        stats.DeptUsagesTW = BuildDeptUsagesForRegion("TW", activeItems);
        stats.DeptUsagesSH = BuildDeptUsagesForRegion("SH", activeItems);

        return stats;
    }

    /// <summary>
    /// 建置指定區域的部門使用矩陣。
    /// 僅納入 appsettings DepartmentDisplay 中設定的 DeptID，
    /// 確保顯示的部門與側邊欄一致，排除不相關的部門。
    /// </summary>
    /// <param name="regionKey">區域代碼 (TW / SH)</param>
    /// <param name="activeItems">已啟用的報表清單</param>
    private List<DeptUsage> BuildDeptUsagesForRegion(string regionKey, List<ReportCatalogItem> activeItems)
    {
        if (!_deptDisplay.Regions.TryGetValue(regionKey, out var regionCfg))
            return [];

        var allowedDeptIds = regionCfg.Depts.Select(d => d.DeptID).ToHashSet();
        var deptMap = new Dictionary<string, (string DeptName, List<ReportCatalogItem> Reports)>();

        foreach (var item in activeItems)
        {
            foreach (var dept in item.Departments)
            {
                var deptIdStr = dept.DeptID.ToString();
                if (!allowedDeptIds.Contains(deptIdStr)) continue;

                if (!deptMap.ContainsKey(deptIdStr))
                    deptMap[deptIdStr] = (dept.DeptName, new List<ReportCatalogItem>());
                deptMap[deptIdStr].Reports.Add(item);
            }
        }

        return deptMap
            .Select(kv => new DeptUsage { DeptName = kv.Value.DeptName, Reports = kv.Value.Reports })
            .ToList();
    }

    // ─── 相依物件 ───

    /// <inheritdoc />
    public List<string> GetAllDependencyObjects()
        => _repo.GetAllDependencyObjects();

    // ─── 報表分類 ───

    /// <inheritdoc />
    public List<ReportCategory> GetCategories()
        => _repo.GetCategories();

    /// <inheritdoc />
    public ReportCategory? GetCategory(int categoryId)
        => _repo.GetCategory(categoryId);

    /// <inheritdoc />
    public ReportCategory SaveCategory(ReportCategory category)
        => _repo.SaveCategory(category);

    /// <inheritdoc />
    public bool DeleteCategory(int categoryId)
        => _repo.DeleteCategory(categoryId);

    // ─── 部門指派 (依公司別過濾) ───

    /// <inheritdoc />
    public List<CatalogDept> GetCatalogDepartments(string companyId)
    {
        // 透過公司別取得對應的 Area，再從 User_Dept 過濾
        var companies = _reportSvc.GetCompanies();
        var company = companies.Find(c => c.Id == companyId);
        var area = company?.Region?.ToUpper() ?? "TW";
        var allDepts = _pksysRepo.GetCatalogDepartments(area);

        // 僅回傳 appsettings DepartmentDisplay 中設定的部門
        if (!_deptDisplay.Regions.TryGetValue(area, out var regionCfg))
            return [];

        var allowedDeptIds = regionCfg.Depts.Select(d => d.DeptID).ToHashSet();
        return allDepts.Where(d => allowedDeptIds.Contains(d.DeptID.ToString())).ToList();
    }

    // ─── 報表資料夾列舉 ───

    /// <inheritdoc />
    public IReadOnlyList<(ReportFolder Value, string Label)> GetReportFolders()
        => ReportFolderExtensions.All();

    // ─── BaseUrl 設定 ───

    /// <inheritdoc />
    public ReportBaseUrlSettings GetBaseUrls()
        => _baseUrls;
}
