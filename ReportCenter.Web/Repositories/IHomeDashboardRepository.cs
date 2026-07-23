namespace ReportCenter.Web.Repositories;

using ReportCenter.Web.Models;

/// <summary>
/// 首頁年度目標戰情 DAL 介面 — 呼叫 usp_Rpt_Home_YtdTarget (ReportCenter DB)。
/// </summary>
public interface IHomeDashboardRepository
{
    /// <summary>
    /// 呼叫 usp_Rpt_Home_YtdTarget，一次取回三區塊 KPI 與逐月趨勢兩個結果集。
    /// </summary>
    /// <param name="mode">模式：'S'=銷售、'O'=接單</param>
    /// <param name="cumulativeType">累計區間：'M'=月結累計、'R'=即時累計</param>
    /// <param name="reportYear">報表年度；NULL = 系統當年度 (首頁固定當年度)</param>
    (List<YtdBlockKpi> Blocks, List<YtdTrendPoint> Trend) GetYtdTarget(
        string mode, string cumulativeType, int? reportYear);
}
