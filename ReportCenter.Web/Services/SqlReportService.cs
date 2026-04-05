namespace ReportCenter.Web.Services;

using Dapper;
using Microsoft.Data.SqlClient;
using ReportCenter.Web.Models;

/// <summary>
/// SQL Server 實作 — 目錄管理走 DB，儀表板/報表明細仍沿用 Mock 資料
/// </summary>
public class SqlReportService : IReportService
{
    private readonly string _connStr;
    private readonly MockReportService _mock = new();

    public SqlReportService(IConfiguration config)
    {
        _connStr = config.GetConnectionString("ReportCenter")
            ?? throw new InvalidOperationException("缺少 ConnectionStrings:ReportCenter 設定");
    }

    private SqlConnection OpenConn()
    {
        var conn = new SqlConnection(_connStr);
        conn.Open();
        return conn;
    }

    // ═══════════════════════════════════════════
    //  儀表板 / 報表 — 沿用 Mock 資料
    // ═══════════════════════════════════════════

    public UserInfo GetCurrentUser() => _mock.GetCurrentUser();
    public List<Company> GetCompanies() => _mock.GetCompanies();
    public List<Department> GetDepartments() => _mock.GetDepartments();
    public Department? GetDepartment(string deptId) => _mock.GetDepartment(deptId);
    public List<Report> GetReports(string deptId) => _mock.GetReports(deptId);
    public Report? GetReport(string deptId, string reportName) => _mock.GetReport(deptId, reportName);
    public List<QuickAccess> GetQuickAccessItems() => _mock.GetQuickAccessItems();
    public List<MaterialRow> GetMaterialRows(string deptId, string reportName, int page = 1, int pageSize = 20)
        => _mock.GetMaterialRows(deptId, reportName, page, pageSize);
    public int GetMaterialRowCount(string deptId, string reportName) => _mock.GetMaterialRowCount(deptId, reportName);
    public DashboardKpi GetDashboardKpi(string companyId) => _mock.GetDashboardKpi(companyId);
    public ChartData GetRevenueChartData(string companyId, string period = "month") => _mock.GetRevenueChartData(companyId, period);
    public ChartData GetDeptComparisonData(string companyId) => _mock.GetDeptComparisonData(companyId);
    public ChartData GetReportChartData(string deptId, string reportName, string chartType)
        => _mock.GetReportChartData(deptId, reportName, chartType);

    // ═══════════════════════════════════════════
    //  報表目錄管理 — SQL Server
    // ═══════════════════════════════════════════

    public List<ReportCatalogItem> GetCatalogItems(string? toolFilter = null, bool? isActive = null, string? search = null)
    {
        using var conn = OpenConn();

        var sql = """
            SELECT c.ReportID, c.ReportName, c.ReportTool, c.ReportPath, c.ReportCode,
                   c.SourceName, c.CreateDate, c.ModifyDate, c.IsActive, c.Remarks,
                   c.CategoryID, cat.CategoryName
            FROM ReportCatalog c
            LEFT JOIN ReportCategory cat ON c.CategoryID = cat.CategoryID
            WHERE 1=1
            """;

        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(toolFilter))
        {
            sql += " AND c.ReportTool = @Tool";
            parameters.Add("Tool", toolFilter);
        }
        if (isActive.HasValue)
        {
            sql += " AND c.IsActive = @IsActive";
            parameters.Add("IsActive", isActive.Value);
        }
        if (!string.IsNullOrEmpty(search))
        {
            sql += " AND (c.ReportName LIKE @Search OR c.SourceName LIKE @Search OR c.ReportPath LIKE @Search)";
            parameters.Add("Search", $"%{search}%");
        }

        sql += " ORDER BY ISNULL(c.ModifyDate, c.CreateDate) DESC";

        var items = conn.Query<ReportCatalogItem>(sql, parameters).ToList();

        // 載入部門指派
        if (items.Count > 0)
        {
            var ids = items.Select(i => i.ReportID).ToList();
            var depts = conn.Query<(int ReportID, int DeptID, string DeptName)>(
                "SELECT ReportID, DeptID, ISNULL(DeptName, '') FROM ReportDepartment WHERE ReportID IN @Ids",
                new { Ids = ids }).ToList();

            var deptMap = depts.GroupBy(d => d.ReportID)
                .ToDictionary(g => g.Key, g => g.Select(d => new DeptAssignment { DeptID = d.DeptID, DeptName = d.DeptName }).ToList());

            foreach (var item in items)
                item.Departments = deptMap.GetValueOrDefault(item.ReportID) ?? [];

            // 載入相依物件
            var deps = conn.Query<(int ReportID, string DependsOn)>(
                "SELECT ReportID, DependsOn FROM ReportDependency WHERE ReportID IN @Ids",
                new { Ids = ids }).ToList();

            var depMap = deps.GroupBy(d => d.ReportID)
                .ToDictionary(g => g.Key, g => g.Select(d => d.DependsOn).ToList());

            foreach (var item in items)
                item.Dependencies = depMap.GetValueOrDefault(item.ReportID) ?? [];
        }

        return items;
    }

    public ReportCatalogItem? GetCatalogItem(int reportId)
    {
        using var conn = OpenConn();

        var item = conn.QueryFirstOrDefault<ReportCatalogItem>(
            """
            SELECT c.ReportID, c.ReportName, c.ReportTool, c.ReportPath, c.ReportCode,
                   c.SourceName, c.CreateDate, c.ModifyDate, c.IsActive, c.Remarks,
                   c.CategoryID, cat.CategoryName
            FROM ReportCatalog c
            LEFT JOIN ReportCategory cat ON c.CategoryID = cat.CategoryID
            WHERE c.ReportID = @ReportID
            """, new { ReportID = reportId });

        if (item == null) return null;

        item.Departments = conn.Query<DeptAssignment>(
            "SELECT DeptID, ISNULL(DeptName, '') AS DeptName FROM ReportDepartment WHERE ReportID = @ReportID",
            new { ReportID = reportId }).ToList();

        item.Dependencies = conn.Query<string>(
            "SELECT DependsOn FROM ReportDependency WHERE ReportID = @ReportID",
            new { ReportID = reportId }).ToList();

        return item;
    }

    public ReportCatalogItem SaveCatalogItem(ReportCatalogItem item)
    {
        using var conn = OpenConn();
        using var tx = conn.BeginTransaction();

        try
        {
            if (item.ReportID == 0)
            {
                // INSERT
                item.ReportID = conn.QuerySingle<int>(
                    """
                    INSERT INTO ReportCatalog (ReportName, ReportTool, ReportPath, ReportCode, SourceName, CreateDate, ModifyDate, IsActive, Remarks, CategoryID)
                    VALUES (@ReportName, @ReportTool, @ReportPath, @ReportCode, @SourceName, GETDATE(), GETDATE(), @IsActive, @Remarks, @CategoryID);
                    SELECT SCOPE_IDENTITY();
                    """,
                    new { item.ReportName, item.ReportTool, item.ReportPath, item.ReportCode, item.SourceName, item.IsActive, item.Remarks, item.CategoryID },
                    tx);
            }
            else
            {
                // UPDATE
                conn.Execute(
                    """
                    UPDATE ReportCatalog SET
                        ReportName = @ReportName, ReportTool = @ReportTool, ReportPath = @ReportPath,
                        ReportCode = @ReportCode, SourceName = @SourceName, ModifyDate = GETDATE(),
                        IsActive = @IsActive, Remarks = @Remarks, CategoryID = @CategoryID
                    WHERE ReportID = @ReportID
                    """,
                    new { item.ReportID, item.ReportName, item.ReportTool, item.ReportPath, item.ReportCode, item.SourceName, item.IsActive, item.Remarks, item.CategoryID },
                    tx);
            }

            // 重建部門指派
            conn.Execute("DELETE FROM ReportDepartment WHERE ReportID = @ReportID", new { item.ReportID }, tx);
            foreach (var dept in item.Departments)
            {
                conn.Execute(
                    "INSERT INTO ReportDepartment (ReportID, DeptID, DeptName) VALUES (@ReportID, @DeptID, @DeptName)",
                    new { item.ReportID, dept.DeptID, dept.DeptName }, tx);
            }

            // 重建相依物件
            conn.Execute("DELETE FROM ReportDependency WHERE ReportID = @ReportID", new { item.ReportID }, tx);
            foreach (var dep in item.Dependencies)
            {
                conn.Execute(
                    "INSERT INTO ReportDependency (ReportID, DependsOn) VALUES (@ReportID, @DependsOn)",
                    new { item.ReportID, DependsOn = dep }, tx);
            }

            tx.Commit();
            return item;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public bool DeleteCatalogItem(int reportId)
    {
        using var conn = OpenConn();
        using var tx = conn.BeginTransaction();

        try
        {
            conn.Execute("DELETE FROM ReportDepartment WHERE ReportID = @ReportID", new { ReportID = reportId }, tx);
            conn.Execute("DELETE FROM ReportDependency WHERE ReportID = @ReportID", new { ReportID = reportId }, tx);
            var affected = conn.Execute("DELETE FROM ReportCatalog WHERE ReportID = @ReportID", new { ReportID = reportId }, tx);
            tx.Commit();
            return affected > 0;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public AdminStats GetAdminStats()
    {
        using var conn = OpenConn();

        var items = GetCatalogItems();

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

    public List<string> GetAllDependencyObjects()
    {
        using var conn = OpenConn();
        return conn.Query<string>("SELECT DISTINCT DependsOn FROM ReportDependency ORDER BY DependsOn").ToList();
    }

    // ═══════════════════════════════════════════
    //  報表分類管理 — SQL Server
    // ═══════════════════════════════════════════

    public List<ReportCategory> GetCategories(int? deptId = null)
    {
        using var conn = OpenConn();
        if (deptId.HasValue)
            return conn.Query<ReportCategory>(
                "SELECT CategoryID, DeptID, CategoryName, ISNULL(SortOrder, 0) AS SortOrder, ISNULL(IsActive, 1) AS IsActive FROM ReportCategory WHERE DeptID = @DeptID ORDER BY SortOrder, CategoryName",
                new { DeptID = deptId.Value }).ToList();

        return conn.Query<ReportCategory>(
            "SELECT CategoryID, DeptID, CategoryName, ISNULL(SortOrder, 0) AS SortOrder, ISNULL(IsActive, 1) AS IsActive FROM ReportCategory ORDER BY DeptID, SortOrder, CategoryName"
        ).ToList();
    }

    public ReportCategory? GetCategory(int categoryId)
    {
        using var conn = OpenConn();
        return conn.QueryFirstOrDefault<ReportCategory>(
            "SELECT CategoryID, DeptID, CategoryName, ISNULL(SortOrder, 0) AS SortOrder, ISNULL(IsActive, 1) AS IsActive FROM ReportCategory WHERE CategoryID = @CategoryID",
            new { CategoryID = categoryId });
    }

    public ReportCategory SaveCategory(ReportCategory category)
    {
        using var conn = OpenConn();
        if (category.CategoryID == 0)
        {
            category.CategoryID = conn.QuerySingle<int>(
                """
                INSERT INTO ReportCategory (DeptID, CategoryName, SortOrder, IsActive)
                VALUES (@DeptID, @CategoryName, @SortOrder, @IsActive);
                SELECT SCOPE_IDENTITY();
                """,
                new { category.DeptID, category.CategoryName, category.SortOrder, category.IsActive });
        }
        else
        {
            conn.Execute(
                """
                UPDATE ReportCategory SET DeptID = @DeptID, CategoryName = @CategoryName,
                       SortOrder = @SortOrder, IsActive = @IsActive
                WHERE CategoryID = @CategoryID
                """,
                new { category.CategoryID, category.DeptID, category.CategoryName, category.SortOrder, category.IsActive });
        }
        return category;
    }

    public bool DeleteCategory(int categoryId)
    {
        using var conn = OpenConn();
        // 先移除報表的 CategoryID 參照
        conn.Execute("UPDATE ReportCatalog SET CategoryID = NULL WHERE CategoryID = @CategoryID", new { CategoryID = categoryId });
        var affected = conn.Execute("DELETE FROM ReportCategory WHERE CategoryID = @CategoryID", new { CategoryID = categoryId });
        return affected > 0;
    }

    // ═══════════════════════════════════════════
    //  部門 (User_Dept) — SQL Server
    // ═══════════════════════════════════════════

    public List<CatalogDept> GetCatalogDepartments(string? area = null)
    {
        using var conn = OpenConn();
        var sql = "SELECT Area, DeptID, DeptName FROM User_Dept WHERE Display = 'Y'";
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(area))
        {
            sql += " AND Area = @Area";
            parameters.Add("Area", area);
        }

        sql += " ORDER BY Area_Sort, Sort";
        return conn.Query<CatalogDept>(sql, parameters).ToList();
    }
}
