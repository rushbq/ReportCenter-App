namespace ReportCenter.Web.Repositories;

using Dapper;
using Microsoft.Data.SqlClient;
using ReportCenter.Web.Models;

/// <summary>
/// SQL Server 實作的報表目錄資料存取層 (DAL)
/// </summary>
public class SqlCatalogRepository : ICatalogRepository
{
    private readonly string _connStr;

    public SqlCatalogRepository(IConfiguration config)
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
    //  報表目錄 CRUD
    //  等 SQL Server 升級後才改用 Entity Framework Core 實作，暫時維持 Dapper 以兼容舊版本
    // ═══════════════════════════════════════════

    public List<ReportCatalogItem> GetCatalogItems(string? toolFilter = null, bool? isActive = null, string? search = null)
    {
        using var conn = OpenConn();

        var sql = """
            SELECT c.ReportID, c.ReportName, c.ReportTool, c.ReportPath, c.ReportCode,
                   c.SourceName, c.CreateDate, c.ModifyDate, c.IsActive, c.Remarks,
                   c.UseCompanyParam, c.CategoryID, cat.CategoryName
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

        if (items.Count > 0)
        {
            var ids = items.Select(i => i.ReportID).ToList();

            // 載入部門指派
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
                   c.UseCompanyParam, c.CategoryID, cat.CategoryName
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
                item.ReportID = conn.QuerySingle<int>(
                    """
                    INSERT INTO ReportCatalog (ReportName, ReportTool, ReportPath, ReportCode, SourceName, CreateDate, ModifyDate, IsActive, Remarks, UseCompanyParam, CategoryID)
                    VALUES (@ReportName, @ReportTool, @ReportPath, @ReportCode, @SourceName, GETDATE(), GETDATE(), @IsActive, @Remarks, @UseCompanyParam, @CategoryID);
                    SELECT SCOPE_IDENTITY();
                    """,
                    new { item.ReportName, item.ReportTool, item.ReportPath, item.ReportCode, item.SourceName, item.IsActive, item.Remarks, item.UseCompanyParam, item.CategoryID },
                    tx);
            }
            else
            {
                conn.Execute(
                    """
                    UPDATE ReportCatalog SET
                        ReportName = @ReportName, ReportTool = @ReportTool, ReportPath = @ReportPath,
                        ReportCode = @ReportCode, SourceName = @SourceName, ModifyDate = GETDATE(),
                        IsActive = @IsActive, Remarks = @Remarks, UseCompanyParam = @UseCompanyParam, CategoryID = @CategoryID
                    WHERE ReportID = @ReportID
                    """,
                    new { item.ReportID, item.ReportName, item.ReportTool, item.ReportPath, item.ReportCode, item.SourceName, item.IsActive, item.Remarks, item.UseCompanyParam, item.CategoryID },
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

    // ═══════════════════════════════════════════
    //  相依物件
    // ═══════════════════════════════════════════

    public List<string> GetAllDependencyObjects()
    {
        using var conn = OpenConn();
        return conn.Query<string>("SELECT DISTINCT DependsOn FROM ReportDependency ORDER BY DependsOn").ToList();
    }

    // ═══════════════════════════════════════════
    //  報表分類 (不含 DeptID)
    // ═══════════════════════════════════════════

    public List<ReportCategory> GetCategories()
    {
        using var conn = OpenConn();
        return conn.Query<ReportCategory>(
            "SELECT CategoryID, CategoryName, ISNULL(SortOrder, 0) AS SortOrder, ISNULL(IsActive, 1) AS IsActive FROM ReportCategory ORDER BY SortOrder, CategoryName"
        ).ToList();
    }

    public ReportCategory? GetCategory(int categoryId)
    {
        using var conn = OpenConn();
        return conn.QueryFirstOrDefault<ReportCategory>(
            "SELECT CategoryID, CategoryName, ISNULL(SortOrder, 0) AS SortOrder, ISNULL(IsActive, 1) AS IsActive FROM ReportCategory WHERE CategoryID = @CategoryID",
            new { CategoryID = categoryId });
    }

    public ReportCategory SaveCategory(ReportCategory category)
    {
        using var conn = OpenConn();
        if (category.CategoryID == 0)
        {
            category.CategoryID = conn.QuerySingle<int>(
                """
                INSERT INTO ReportCategory (CategoryName, SortOrder, IsActive)
                VALUES (@CategoryName, @SortOrder, @IsActive);
                SELECT SCOPE_IDENTITY();
                """,
                new { category.CategoryName, category.SortOrder, category.IsActive });
        }
        else
        {
            conn.Execute(
                """
                UPDATE ReportCategory SET CategoryName = @CategoryName,
                       SortOrder = @SortOrder, IsActive = @IsActive
                WHERE CategoryID = @CategoryID
                """,
                new { category.CategoryID, category.CategoryName, category.SortOrder, category.IsActive });
        }
        return category;
    }

    public bool DeleteCategory(int categoryId)
    {
        using var conn = OpenConn();
        conn.Execute("UPDATE ReportCatalog SET CategoryID = NULL WHERE CategoryID = @CategoryID", new { CategoryID = categoryId });
        var affected = conn.Execute("DELETE FROM ReportCategory WHERE CategoryID = @CategoryID", new { CategoryID = categoryId });
        return affected > 0;
    }

    // ═══════════════════════════════════════════
    //  部門 (User_Dept)
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

    // ═══════════════════════════════════════════
    //  前端查詢 (依部門取啟用中報表)
    // ═══════════════════════════════════════════

    public List<ReportCatalogItem> GetActiveReportsByDepartment(int deptId)
    {
        using var conn = OpenConn();

        var items = conn.Query<ReportCatalogItem>(
            """
            SELECT c.ReportID, c.ReportName, c.ReportTool, c.ReportPath, c.ReportCode,
                   c.SourceName, c.CreateDate, c.ModifyDate, c.IsActive, c.Remarks,
                   c.UseCompanyParam, c.CategoryID, cat.CategoryName
            FROM ReportCatalog c
            INNER JOIN ReportDepartment rd ON c.ReportID = rd.ReportID
            LEFT JOIN ReportCategory cat ON c.CategoryID = cat.CategoryID
            WHERE c.IsActive = 1 AND rd.DeptID = @DeptID
            ORDER BY ISNULL(c.ModifyDate, c.CreateDate) DESC
            """, new { DeptID = deptId }).ToList();

        return items;
    }

    public List<string> GetCategoriesByDepartment(int deptId)
    {
        using var conn = OpenConn();

        return conn.Query<string>(
            """
            SELECT DISTINCT cat.CategoryName
            FROM ReportCatalog c
            INNER JOIN ReportDepartment rd ON c.ReportID = rd.ReportID
            INNER JOIN ReportCategory cat ON c.CategoryID = cat.CategoryID
            WHERE c.IsActive = 1 AND rd.DeptID = @DeptID AND cat.CategoryName IS NOT NULL
            ORDER BY cat.CategoryName
            """, new { DeptID = deptId }).ToList();
    }

    public int GetActiveReportCountByDepartment(int deptId)
    {
        using var conn = OpenConn();

        return conn.QuerySingle<int>(
            """
            SELECT COUNT(DISTINCT c.ReportID)
            FROM ReportCatalog c
            INNER JOIN ReportDepartment rd ON c.ReportID = rd.ReportID
            WHERE c.IsActive = 1 AND rd.DeptID = @DeptID
            """, new { DeptID = deptId });
    }

    // ═══════════════════════════════════════════
    //  使用者收藏
    // ═══════════════════════════════════════════

    public List<int> GetUserFavorites(string userId)
    {
        using var conn = OpenConn();
        return conn.Query<int>(
            "SELECT ReportID FROM UserFavorite WHERE UserID = @UserID",
            new { UserID = userId }).ToList();
    }

    public void AddFavorite(string userId, int reportId)
    {
        using var conn = OpenConn();
        conn.Execute(
            """
            IF NOT EXISTS (SELECT 1 FROM UserFavorite WHERE UserID = @UserID AND ReportID = @ReportID)
                INSERT INTO UserFavorite (UserID, ReportID) VALUES (@UserID, @ReportID)
            """,
            new { UserID = userId, ReportID = reportId });
    }

    public void RemoveFavorite(string userId, int reportId)
    {
        using var conn = OpenConn();
        conn.Execute(
            "DELETE FROM UserFavorite WHERE UserID = @UserID AND ReportID = @ReportID",
            new { UserID = userId, ReportID = reportId });
    }

    // ═══════════════════════════════════════════
    //  使用者釘選
    // ═══════════════════════════════════════════

    public List<ReportCatalogItem> GetUserPins(string userId)
    {
        using var conn = OpenConn();

        return conn.Query<ReportCatalogItem>(
            """
            SELECT c.ReportID, c.ReportName, c.ReportTool, c.ReportPath, c.ReportCode,
                   c.SourceName, c.CreateDate, c.ModifyDate, c.IsActive, c.Remarks,
                   c.UseCompanyParam, c.CategoryID, cat.CategoryName
            FROM UserPin p
            INNER JOIN ReportCatalog c ON p.ReportID = c.ReportID
            LEFT JOIN ReportCategory cat ON c.CategoryID = cat.CategoryID
            WHERE p.UserID = @UserID AND c.IsActive = 1
            ORDER BY p.SortOrder, p.CreatedDate
            """, new { UserID = userId }).ToList();
    }

    public void AddPin(string userId, int reportId)
    {
        using var conn = OpenConn();
        conn.Execute(
            """
            IF NOT EXISTS (SELECT 1 FROM UserPin WHERE UserID = @UserID AND ReportID = @ReportID)
            BEGIN
                DECLARE @MaxSort INT = ISNULL((SELECT MAX(SortOrder) FROM UserPin WHERE UserID = @UserID), 0);
                INSERT INTO UserPin (UserID, ReportID, SortOrder) VALUES (@UserID, @ReportID, @MaxSort + 1)
            END
            """,
            new { UserID = userId, ReportID = reportId });
    }

    public void RemovePin(string userId, int reportId)
    {
        using var conn = OpenConn();
        conn.Execute(
            "DELETE FROM UserPin WHERE UserID = @UserID AND ReportID = @ReportID",
            new { UserID = userId, ReportID = reportId });
    }
}
