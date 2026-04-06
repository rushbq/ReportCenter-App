namespace ReportCenter.Web.Services;

using Microsoft.Extensions.Options;
using ReportCenter.Web.Models;
using ReportCenter.Web.Models.Enums;
using ReportCenter.Web.Models.Settings;
using ReportCenter.Web.Repositories;

/// <summary>
/// 報表目錄管理商業邏輯實作 (BLL)
/// </summary>
public class CatalogService : ICatalogService
{
    private readonly ICatalogRepository _repo;
    private readonly IReportService _reportSvc;
    private readonly ReportBaseUrlSettings _baseUrls;
    private readonly DepartmentDisplaySettings _deptDisplay;

    public CatalogService(ICatalogRepository repo, IReportService reportSvc,
        IOptions<ReportBaseUrlSettings> baseUrls, IOptions<DepartmentDisplaySettings> deptDisplay)
    {
        _repo = repo;
        _reportSvc = reportSvc;
        _baseUrls = baseUrls.Value;
        _deptDisplay = deptDisplay.Value;
    }

    // ─── 報表目錄 CRUD ───

    public List<ReportCatalogItem> GetCatalogItems(string? toolFilter = null, bool? isActive = null, string? search = null)
        => _repo.GetCatalogItems(toolFilter, isActive, search);

    public ReportCatalogItem? GetCatalogItem(int reportId)
        => _repo.GetCatalogItem(reportId);

    public ReportCatalogItem SaveCatalogItem(ReportCatalogItem item)
        => _repo.SaveCatalogItem(item);

    public bool DeleteCatalogItem(int reportId)
        => _repo.DeleteCatalogItem(reportId);

    public bool ToggleCatalogItem(int reportId)
    {
        var item = _repo.GetCatalogItem(reportId);
        if (item == null) return false;
        item.IsActive = !item.IsActive;
        _repo.SaveCatalogItem(item);
        return true;
    }

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

        // 從 appsettings DepartmentDisplay 取得各區域允許的 DeptID
        var allowedTW = _deptDisplay.Regions.TryGetValue("TW", out var twCfg)
            ? twCfg.Depts.Select(d => d.DeptID).ToHashSet() : [];
        var allowedSH = _deptDisplay.Regions.TryGetValue("SH", out var shCfg)
            ? shCfg.Depts.Select(d => d.DeptID).ToHashSet() : [];

        // 查詢 User_Dept 取得 DeptID → DeptName 對照（含 Area）
        var allCatalogDepts = _repo.GetCatalogDepartments("TW").Concat(_repo.GetCatalogDepartments("SH")).ToList();
        var deptLookup = allCatalogDepts.ToDictionary(d => d.DeptID, d => d);

        // 建立部門使用矩陣（依 DeptID 分組，僅含 appsettings 允許的部門）
        var deptMapTW = new Dictionary<string, (string DeptName, List<ReportCatalogItem> Reports)>();
        var deptMapSH = new Dictionary<string, (string DeptName, List<ReportCatalogItem> Reports)>();

        foreach (var item in items.Where(i => i.IsActive))
        {
            foreach (var dept in item.Departments)
            {
                var deptIdStr = dept.DeptID.ToString();

                if (allowedTW.Contains(deptIdStr))
                {
                    if (!deptMapTW.ContainsKey(deptIdStr))
                        deptMapTW[deptIdStr] = (dept.DeptName, []);
                    deptMapTW[deptIdStr].Reports.Add(item);
                }
                else if (allowedSH.Contains(deptIdStr))
                {
                    if (!deptMapSH.ContainsKey(deptIdStr))
                        deptMapSH[deptIdStr] = (dept.DeptName, []);
                    deptMapSH[deptIdStr].Reports.Add(item);
                }
            }
        }

        stats.DeptUsagesTW = deptMapTW.Select(kv => new DeptUsage { DeptName = kv.Value.DeptName, Reports = kv.Value.Reports }).ToList();
        stats.DeptUsagesSH = deptMapSH.Select(kv => new DeptUsage { DeptName = kv.Value.DeptName, Reports = kv.Value.Reports }).ToList();

        return stats;
    }

    // ─── 相依物件 ───

    public List<string> GetAllDependencyObjects()
        => _repo.GetAllDependencyObjects();

    // ─── 報表分類 ───

    public List<ReportCategory> GetCategories()
        => _repo.GetCategories();

    public ReportCategory? GetCategory(int categoryId)
        => _repo.GetCategory(categoryId);

    public ReportCategory SaveCategory(ReportCategory category)
        => _repo.SaveCategory(category);

    public bool DeleteCategory(int categoryId)
        => _repo.DeleteCategory(categoryId);

    // ─── 部門指派 (依公司別過濾) ───

    public List<CatalogDept> GetCatalogDepartments(string companyId)
    {
        // 透過公司別取得對應的 Area，再從 User_Dept 過濾
        var companies = _reportSvc.GetCompanies();
        var company = companies.Find(c => c.Id == companyId);
        var area = company?.Region;
        return _repo.GetCatalogDepartments(area);
    }

    // ─── 報表資料夾列舉 ───

    public IReadOnlyList<(ReportFolder Value, string Label)> GetReportFolders()
        => ReportFolderExtensions.All();

    // ─── BaseUrl 設定 ───

    public ReportBaseUrlSettings GetBaseUrls()
        => _baseUrls;
}
