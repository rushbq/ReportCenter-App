namespace ReportCenter.Web.Services;

using ReportCenter.Web.Models;
using ReportCenter.Web.Repositories;

/// <summary>
/// 權限管理商業邏輯實作 (BLL)。
/// 整合 PermissionRepository (ReportCenter DB) 與 PksysRepository (PKSYS DB)，
/// 提供權限 CRUD 及 UI 所需的樹狀資料。
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permRepo;
    private readonly IPksysRepository _pksysRepo;
    private readonly ICatalogRepository _catalogRepo;
    private readonly HashSet<string> _adminUsers;

    public PermissionService(
        IPermissionRepository permRepo,
        IPksysRepository pksysRepo,
        ICatalogRepository catalogRepo,
        IConfiguration config)
    {
        _permRepo = permRepo;
        _pksysRepo = pksysRepo;
        _catalogRepo = catalogRepo;

        // 從 appsettings.json 的 AdminUsers 區段讀取管理員白名單
        _adminUsers = config.GetSection("AdminUsers")
            .Get<string[]>()?
            .ToHashSet(StringComparer.OrdinalIgnoreCase)
            ?? [];
    }

    // ─── 權限操作 ───

    /// <inheritdoc />
    public int GrantBatch(List<string> employeeIds, List<int> reportIds, string grantedBy)
        => _permRepo.GrantPermissions(employeeIds, reportIds, grantedBy);

    /// <inheritdoc />
    public List<UserReportPermission> GetUserPermissions(string employeeId)
        => _permRepo.GetPermissionsByUser(employeeId);

    /// <inheritdoc />
    public bool RevokePermission(string employeeId, int reportId)
        => _permRepo.RevokePermission(employeeId, reportId);

    /// <inheritdoc />
    public bool HasPermission(string employeeId, int reportId)
        => _permRepo.HasPermission(employeeId, reportId);

    /// <inheritdoc />
    public List<int> GetAuthorizedReportIds(string employeeId)
        => _permRepo.GetAuthorizedReportIds(employeeId);

    // ─── UI 樹狀資料 ───

    /// <inheritdoc />
    public List<DeptWithUsers> GetDeptUserTree()
    {
        var allDepts = _pksysRepo.GetCatalogDepartments();
        var result = new List<DeptWithUsers>();

        foreach (var dept in allDepts)
        {
            var users = _pksysRepo.GetUsersByDepartment(dept.DeptID);
            if (users.Count == 0) continue;

            result.Add(new DeptWithUsers
            {
                Area = dept.Area,
                DeptID = dept.DeptID,
                DeptName = dept.DeptName,
                Users = users,
            });
        }

        return result;
    }

    /// <inheritdoc />
    public List<CategoryWithReports> GetReportTree()
    {
        var items = _catalogRepo.GetCatalogItems(isActive: true);
        var categories = _catalogRepo.GetCategories()
            .Where(c => c.IsActive)
            .ToList();

        var result = new List<CategoryWithReports>();

        // 有分類的報表
        foreach (var cat in categories)
        {
            var reports = items
                .Where(i => i.CategoryID == cat.CategoryID)
                .Select(i => new ReportTreeItem
                {
                    ReportID = i.ReportID,
                    ReportName = i.ReportName,
                    ReportTool = i.ReportTool,
                    IsActive = i.IsActive,
                })
                .ToList();

            if (reports.Count == 0) continue;

            result.Add(new CategoryWithReports
            {
                CategoryID = cat.CategoryID,
                CategoryName = cat.CategoryName,
                Reports = reports,
            });
        }

        // 未分類的報表
        var uncategorized = items
            .Where(i => i.CategoryID == null || i.CategoryID == 0)
            .Select(i => new ReportTreeItem
            {
                ReportID = i.ReportID,
                ReportName = i.ReportName,
                ReportTool = i.ReportTool,
                IsActive = i.IsActive,
            })
            .ToList();

        if (uncategorized.Count > 0)
        {
            result.Add(new CategoryWithReports
            {
                CategoryID = 0,
                CategoryName = "未分類",
                Reports = uncategorized,
            });
        }

        return result;
    }

    /// <inheritdoc />
    public List<UserProfileItem> SearchUsers(string? keyword)
        => _pksysRepo.SearchUsers(keyword);

    // ─── 管理員權限 ───

    /// <inheritdoc />
    public bool IsAdmin(string employeeId)
        => _adminUsers.Contains(employeeId);
}
