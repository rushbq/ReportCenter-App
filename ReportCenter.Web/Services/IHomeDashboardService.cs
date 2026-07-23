namespace ReportCenter.Web.Services;

using ReportCenter.Web.Models;

/// <summary>
/// 首頁年度目標戰情 BLL 介面。
/// </summary>
public interface IHomeDashboardService
{
    /// <summary>
    /// 取得首頁年度目標戰情 (三卡 KPI + 逐月趨勢)。年度固定系統當年度。
    /// </summary>
    /// <param name="mode">模式：S=銷售、O=接單 (非法值退回 S)</param>
    /// <param name="cumulativeType">區間：M=月結累計、R=即時累計 (非法值退回 M)</param>
    HomeYtdDashboard GetYtdTarget(string? mode, string? cumulativeType);
}
