namespace ReportCenter.Web.Repositories;

using Dapper;
using Microsoft.Data.SqlClient;
using ReportCenter.Web.Models;

/// <summary>
/// PKSYS 資料庫 Dapper 實作 — User_Dept / User_Profile
/// </summary>
public class SqlPksysRepository : IPksysRepository
{
    private readonly string _connStr;

    public SqlPksysRepository(IConfiguration config)
    {
        _connStr = config.GetConnectionString("PKSYS")
            ?? throw new InvalidOperationException("缺少 ConnectionStrings:PKSYS 設定");
    }

    private SqlConnection OpenConn()
    {
        var conn = new SqlConnection(_connStr);
        conn.Open();
        return conn;
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
    //  使用者 (User_Profile)
    // ═══════════════════════════════════════════

    public List<UserProfileItem> SearchUsers(string? keyword = null, string? deptId = null)
    {
        using var conn = OpenConn();
        var sql = """
            SELECT p.Account_Name AS AccountName, p.Display_Name AS DisplayName,
                   p.DeptID, ISNULL(d.DeptName, '') AS DeptName
            FROM User_Profile p
            LEFT JOIN User_Dept d ON p.DeptID = d.DeptID
            WHERE p.Display = 'Y'
            """;
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(keyword))
        {
            sql += " AND (p.Account_Name LIKE @Keyword OR p.Display_Name LIKE @Keyword)";
            parameters.Add("Keyword", $"%{keyword}%");
        }

        if (!string.IsNullOrEmpty(deptId))
        {
            sql += " AND p.DeptID = @DeptID";
            parameters.Add("DeptID", deptId);
        }

        sql += " ORDER BY p.DeptID, p.Account_Name";
        return conn.Query<UserProfileItem>(sql, parameters).ToList();
    }

    public UserProfileItem? GetUser(string accountName)
    {
        using var conn = OpenConn();
        return conn.QueryFirstOrDefault<UserProfileItem>(
            """
            SELECT p.Account_Name AS AccountName, p.Display_Name AS DisplayName,
                   p.DeptID, ISNULL(d.DeptName, '') AS DeptName
            FROM User_Profile p
            LEFT JOIN User_Dept d ON p.DeptID = d.DeptID
            WHERE p.Display = 'Y' AND p.Account_Name = @AccountName
            """, new { AccountName = accountName });
    }

    public List<UserProfileItem> GetUsersByDepartment(string deptId)
    {
        using var conn = OpenConn();
        return conn.Query<UserProfileItem>(
            """
            SELECT p.Account_Name AS AccountName, p.Display_Name AS DisplayName,
                   p.DeptID, ISNULL(d.DeptName, '') AS DeptName
            FROM User_Profile p
            LEFT JOIN User_Dept d ON p.DeptID = d.DeptID
            WHERE p.Display = 'Y' AND p.DeptID = @DeptID
            ORDER BY p.Account_Name
            """, new { DeptID = deptId }).ToList();
    }
}
