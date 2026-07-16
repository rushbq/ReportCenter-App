namespace ReportCenter.Web.Services;

using ReportCenter.Web.Models;

/// <summary>
/// 報表使用記錄 BLL 介面 — 點擊記錄與使用分析
/// </summary>
public interface IUsageService
{
    /// <summary>
    /// 記錄目前登入者的一次報表點擊 (30 秒內重複點擊不記錄)。
    /// 回傳是否實際寫入。
    /// </summary>
    bool TrackClick(int reportId, string? source);

    /// <summary>取得使用分析總覽 (KPI、30 日趨勢、熱門 Top、冷門清單)</summary>
    UsageOverview GetOverview();

    /// <summary>分頁查詢使用明細 (使用者姓名/編號模糊篩選、報表、日期區間)</summary>
    UsageLogPage GetLogs(UsageLogQuery query);

    /// <summary>取得冷門報表清單 (啟用中且 90 日內 0 次使用)，供管理總覽警示用</summary>
    List<ReportUsageSummary> GetColdReports();

    /// <summary>取得近 30 日各報表使用彙總對照 (ReportID → 彙總)，供目錄管理清單欄位用</summary>
    Dictionary<int, ReportUsageSummary> GetRecentUsageMap();
}
