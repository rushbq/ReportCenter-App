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

    /// <remarks>
    /// 集合式寫入：「員編 × 報表」一次 INSERT … SELECT，已存在的組合以 NOT EXISTS 略過，
    /// 回傳值即實際新增筆數。原本逐筆 IF NOT EXISTS … INSERT 有兩個問題：
    /// 往返次數 = 人數 × 報表數 (交易鎖定時間隨之拉長)；略過時 Execute 回傳 -1，新增數統計錯誤。
    /// 不存在的 ReportID 會因 JOIN ReportCatalog 被自然略過 (原本會 FK 失敗整批 rollback)。
    /// 員編以 VALUES 列建構式帶入 (SQL Server 2008 起支援)；受單一命令參數上限 2100、
    /// VALUES 上限 1000 列限制，員編依報表數分批，仍在同一交易內。
    /// </remarks>
    public int GrantPermissions(List<string> employeeIds, List<int> reportIds, string grantedBy)
    {
        // 去重：同一批 INSERT 內重複的組合會違反 UQ_UserReport (比照 DB 定序忽略大小寫)
        var emps = employeeIds.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var rpts = reportIds.Distinct().ToList();
        if (emps.Count == 0 || rpts.Count == 0) return 0;

        using var conn = OpenConn();
        using var tx = conn.BeginTransaction();
        var count = 0;

        try
        {
            var chunkSize = Math.Clamp(2000 - rpts.Count, 1, 1000);
            foreach (var chunk in emps.Chunk(chunkSize))
            {
                var parameters = new DynamicParameters();
                parameters.Add("GrantedBy", grantedBy);
                parameters.Add("ReportIds", rpts);

                var rows = new List<string>(chunk.Length);
                for (var i = 0; i < chunk.Length; i++)
                {
                    parameters.Add($"E{i}", chunk[i]);
                    rows.Add($"(@E{i})");
                }

                count += conn.Execute(
                    $"""
                    INSERT INTO UserReportPermission (EmployeeId, ReportID, GrantedBy)
                    SELECT e.EmployeeId, c.ReportID, @GrantedBy
                    FROM (VALUES {string.Join(", ", rows)}) AS e (EmployeeId)
                    CROSS JOIN ReportCatalog c
                    WHERE c.ReportID IN @ReportIds
                      AND NOT EXISTS (
                          SELECT 1 FROM UserReportPermission p
                          WHERE p.EmployeeId = e.EmployeeId AND p.ReportID = c.ReportID
                      )
                    """,
                    parameters, tx);
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

    public List<int> GetAuthorizedReportIds(string employeeId)
    {
        using var conn = OpenConn();
        return conn.Query<int>(
            "SELECT ReportID FROM UserReportPermission WHERE EmployeeId = @EmployeeId",
            new { EmployeeId = employeeId }).ToList();
    }
}
