namespace ReportCenter.Web.Repositories;

using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ReportCenter.Web.Models;

/// <summary>
/// 首頁年度目標戰情 Dapper 實作 — ReportCenter 資料庫。
/// usp_Rpt_Home_YtdTarget 回兩個結果集，故以 QueryMultiple 讀取；
/// 不可被外層 INSERT...EXEC 包起來 (SQL Server 限制)。
/// </summary>
public class SqlHomeDashboardRepository : IHomeDashboardRepository
{
    private readonly string _connStr;

    public SqlHomeDashboardRepository(IConfiguration config)
    {
        _connStr = config.GetConnectionString("ReportCenter")
            ?? throw new InvalidOperationException("缺少 ConnectionStrings:ReportCenter 設定");
    }

    public (List<YtdBlockKpi> Blocks, List<YtdTrendPoint> Trend) GetYtdTarget(
        string mode, string cumulativeType, int? reportYear)
    {
        using var conn = new SqlConnection(_connStr);
        conn.Open();

        using var multi = conn.QueryMultiple(
            "dbo.usp_Rpt_Home_YtdTarget",
            new { Mode = mode, CumulativeType = cumulativeType, ReportYear = reportYear },
            commandType: CommandType.StoredProcedure);

        var blocks = multi.Read<YtdBlockKpi>().ToList();   // 結果集 1：三區塊 KPI
        var trend = multi.Read<YtdTrendPoint>().ToList();  // 結果集 2：逐月趨勢
        return (blocks, trend);
    }
}
