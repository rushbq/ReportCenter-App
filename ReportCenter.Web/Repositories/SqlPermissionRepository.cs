namespace ReportCenter.Web.Repositories;

using Dapper;
using Microsoft.Data.SqlClient;
using ReportCenter.Web.Models;

/// <summary>
/// 使用者報表權限 Dapper 實作 — ReportCenter 資料庫
/// </summary>
public class SqlPermissionRepository : IPermissionRepository
{
    private readonly string _connStr;

    public SqlPermissionRepository(IConfiguration config)
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

    public List<UserReportPermission> GetPermissionsByUser(string employeeId)
    {
        using var conn = OpenConn();
        return conn.Query<UserReportPermission>(
            """
            SELECT p.PermissionID, p.EmployeeId, p.ReportID, p.GrantedBy, p.GrantedDate,
                   c.ReportName, ISNULL(cat.CategoryName, '') AS CategoryName, c.ReportTool
            FROM UserReportPermission p
            INNER JOIN ReportCatalog c ON p.ReportID = c.ReportID
            LEFT JOIN ReportCategory cat ON c.CategoryID = cat.CategoryID
            WHERE p.EmployeeId = @EmployeeId
            ORDER BY cat.CategoryName, c.ReportName
            """, new { EmployeeId = employeeId }).ToList();
    }

    public List<UserReportPermission> GetPermissionsByReport(int reportId)
    {
        using var conn = OpenConn();
        return conn.Query<UserReportPermission>(
            """
            SELECT p.PermissionID, p.EmployeeId, p.ReportID, p.GrantedBy, p.GrantedDate,
                   c.ReportName, ISNULL(cat.CategoryName, '') AS CategoryName, c.ReportTool
            FROM UserReportPermission p
            INNER JOIN ReportCatalog c ON p.ReportID = c.ReportID
            LEFT JOIN ReportCategory cat ON c.CategoryID = cat.CategoryID
            WHERE p.ReportID = @ReportID
            ORDER BY p.EmployeeId
            """, new { ReportID = reportId }).ToList();
    }

    public int GrantPermissions(List<string> employeeIds, List<int> reportIds, string grantedBy)
    {
        using var conn = OpenConn();
        using var tx = conn.BeginTransaction();
        var count = 0;

        try
        {
            foreach (var empId in employeeIds)
            {
                foreach (var reportId in reportIds)
                {
                    var affected = conn.Execute(
                        """
                        IF NOT EXISTS (
                            SELECT 1 FROM UserReportPermission
                            WHERE EmployeeId = @EmployeeId AND ReportID = @ReportID
                        )
                        INSERT INTO UserReportPermission (EmployeeId, ReportID, GrantedBy)
                        VALUES (@EmployeeId, @ReportID, @GrantedBy)
                        """,
                        new { EmployeeId = empId, ReportID = reportId, GrantedBy = grantedBy },
                        tx);
                    count += affected;
                }
            }

            tx.Commit();
            return count;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public bool RevokePermission(string employeeId, int reportId)
    {
        using var conn = OpenConn();
        var affected = conn.Execute(
            "DELETE FROM UserReportPermission WHERE EmployeeId = @EmployeeId AND ReportID = @ReportID",
            new { EmployeeId = employeeId, ReportID = reportId });
        return affected > 0;
    }

    public bool HasPermission(string employeeId, int reportId)
    {
        using var conn = OpenConn();
        return conn.QuerySingleOrDefault<int>(
            "SELECT COUNT(1) FROM UserReportPermission WHERE EmployeeId = @EmployeeId AND ReportID = @ReportID",
            new { EmployeeId = employeeId, ReportID = reportId }) > 0;
    }

    public List<int> GetAuthorizedReportIds(string employeeId)
    {
        using var conn = OpenConn();
        return conn.Query<int>(
            "SELECT ReportID FROM UserReportPermission WHERE EmployeeId = @EmployeeId",
            new { EmployeeId = employeeId }).ToList();
    }
}
