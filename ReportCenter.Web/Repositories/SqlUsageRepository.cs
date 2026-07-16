namespace ReportCenter.Web.Repositories;

using Dapper;
using Microsoft.Data.SqlClient;
using ReportCenter.Web.Models;

/// <summary>
/// 報表使用記錄 Dapper 實作 — ReportCenter 資料庫
/// </summary>
public class SqlUsageRepository : IUsageRepository
{
    private readonly string _connStr;

    public SqlUsageRepository(IConfiguration config)
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

    public bool TryInsert(int reportId, string employeeId, string companyId, string source, int dedupeSeconds)
    {
        using var conn = OpenConn();
        var affected = conn.Execute(
            """
            IF NOT EXISTS (
                SELECT 1 FROM ReportUsageLog
                WHERE EmployeeId = @EmployeeId AND ReportID = @ReportID
                  AND ClickedAt >= DATEADD(SECOND, -@DedupeSeconds, SYSDATETIME())
            )
            INSERT INTO ReportUsageLog (ReportID, EmployeeId, CompanyId, Source)
            VALUES (@ReportID, @EmployeeId, @CompanyId, @Source)
            """,
            new { ReportID = reportId, EmployeeId = employeeId, CompanyId = companyId, Source = source, DedupeSeconds = dedupeSeconds });
        return affected > 0;
    }

    public (int Clicks, int Users) GetTotals(DateTime since)
    {
        using var conn = OpenConn();
        var row = conn.QuerySingle<(int, int)>(
            """
            SELECT COUNT(*), COUNT(DISTINCT EmployeeId)
            FROM ReportUsageLog
            WHERE ClickedAt >= @Since
            """, new { Since = since });
        return row;
    }

    public List<DailyUsageCount> GetDailyCounts(DateTime since)
    {
        using var conn = OpenConn();
        return conn.Query<DailyUsageCount>(
            """
            SELECT CAST(ClickedAt AS DATE) AS [Date], COUNT(*) AS [Count]
            FROM ReportUsageLog
            WHERE ClickedAt >= @Since
            GROUP BY CAST(ClickedAt AS DATE)
            ORDER BY [Date]
            """, new { Since = since }).ToList();
    }

    public List<ReportUsageSummary> GetReportSummaries(DateTime since)
    {
        using var conn = OpenConn();
        return conn.Query<ReportUsageSummary>(
            """
            SELECT u.ReportID,
                   c.ReportName,
                   c.ReportTool,
                   ISNULL(cat.CategoryName, '') AS CategoryName,
                   COUNT(*)                     AS Clicks,
                   COUNT(DISTINCT u.EmployeeId) AS Users,
                   MAX(u.ClickedAt)             AS LastUsed
            FROM ReportUsageLog u
            INNER JOIN ReportCatalog c ON u.ReportID = c.ReportID
            LEFT JOIN ReportCategory cat ON c.CategoryID = cat.CategoryID
            WHERE u.ClickedAt >= @Since
            GROUP BY u.ReportID, c.ReportName, c.ReportTool, cat.CategoryName
            ORDER BY Clicks DESC, LastUsed DESC
            """, new { Since = since }).ToList();
    }

    public List<ReportUsageSummary> GetColdReports(DateTime since)
    {
        using var conn = OpenConn();
        return conn.Query<ReportUsageSummary>(
            """
            SELECT c.ReportID,
                   c.ReportName,
                   c.ReportTool,
                   ISNULL(cat.CategoryName, '') AS CategoryName,
                   0                            AS Clicks,
                   0                            AS Users,
                   (SELECT MAX(u.ClickedAt) FROM ReportUsageLog u WHERE u.ReportID = c.ReportID) AS LastUsed
            FROM ReportCatalog c
            LEFT JOIN ReportCategory cat ON c.CategoryID = cat.CategoryID
            WHERE c.IsActive = 1
              AND NOT EXISTS (
                  SELECT 1 FROM ReportUsageLog u
                  WHERE u.ReportID = c.ReportID AND u.ClickedAt >= @Since
              )
            ORDER BY LastUsed ASC, c.ReportName
            """, new { Since = since }).ToList();
    }

    public int GetActiveReportCount()
    {
        using var conn = OpenConn();
        return conn.QuerySingle<int>("SELECT COUNT(*) FROM ReportCatalog WHERE IsActive = 1");
    }

    public UsageLogPage GetLogs(int? reportId, List<string>? employeeIds, DateTime? from, DateTime? to, int page, int pageSize)
    {
        using var conn = OpenConn();

        var where = "WHERE 1 = 1";
        if (reportId.HasValue) where += " AND u.ReportID = @ReportID";
        if (employeeIds != null) where += " AND u.EmployeeId IN @EmployeeIds";
        if (from.HasValue) where += " AND u.ClickedAt >= @From";
        if (to.HasValue) where += " AND u.ClickedAt < @To";

        var param = new
        {
            ReportID = reportId,
            EmployeeIds = employeeIds,
            From = from,
            To = to,
            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        };

        var total = conn.QuerySingle<int>(
            $"SELECT COUNT(*) FROM ReportUsageLog u {where}", param);

        // 分頁採 ROW_NUMBER() 而非 OFFSET/FETCH — 後者為 SQL Server 2012+ 語法，
        // 正式環境為 2008 R2 (compatibility level 100) 不支援。詳見 AGENTS.md 相容性規範。
        var items = conn.Query<UsageLogItem>(
            $"""
            SELECT t.LogID, t.ReportID, t.EmployeeId, t.CompanyId, t.Source, t.ClickedAt, t.ReportName
            FROM (
                SELECT u.LogID, u.ReportID, u.EmployeeId, u.CompanyId, u.Source, u.ClickedAt,
                       c.ReportName,
                       ROW_NUMBER() OVER (ORDER BY u.ClickedAt DESC, u.LogID DESC) AS RowNum
                FROM ReportUsageLog u
                INNER JOIN ReportCatalog c ON u.ReportID = c.ReportID
                {where}
            ) AS t
            WHERE t.RowNum > @Offset AND t.RowNum <= @Offset + @PageSize
            ORDER BY t.RowNum
            """, param).ToList();

        return new UsageLogPage { Total = total, Page = page, PageSize = pageSize, Items = items };
    }
}
