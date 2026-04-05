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

    public CatalogService(ICatalogRepository repo, IReportService reportSvc, IOptions<ReportBaseUrlSettings> baseUrls)
    {
        _repo = repo;
        _reportSvc = reportSvc;
        _baseUrls = baseUrls.Value;
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

        // 部門使用矩陣
        var deptMap = new Dictionary<string, List<ReportCatalogItem>>();
        foreach (var item in items.Where(i => i.IsActive))
        {
            foreach (var dept in item.Departments)
            {
                if (!deptMap.ContainsKey(dept.DeptName))
                    deptMap[dept.DeptName] = [];
                deptMap[dept.DeptName].Add(item);
            }
        }
        stats.DeptUsages = deptMap.Select(kv => new DeptUsage { DeptName = kv.Key, Reports = kv.Value }).ToList();

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
