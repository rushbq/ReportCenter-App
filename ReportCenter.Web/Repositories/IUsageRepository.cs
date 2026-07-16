namespace ReportCenter.Web.Repositories;

using ReportCenter.Web.Models;

/// <summary>
/// 報表使用記錄資料存取介面 (DAL) — ReportCenter 資料庫 ReportUsageLog
/// </summary>
public interface IUsageRepository
{
    /// <summary>
    /// 寫入一筆使用記錄；同一使用者對同一報表在 dedupeSeconds 秒內重複點擊則略過。
    /// 回傳是否實際寫入。
    /// </summary>
    bool TryInsert(int reportId, string employeeId, string companyId, string source, int dedupeSeconds);

    /// <summary>取得期間內總點擊數與活躍使用者數</summary>
    (int Clicks, int Users) GetTotals(DateTime since);

    /// <summary>取得期間內每日點擊數 (趨勢圖用，僅回傳有資料的日期)</summary>
    List<DailyUsageCount> GetDailyCounts(DateTime since);

    /// <summary>取得期間內有點擊的報表彙總 (點擊數、使用人數、最後使用時間)，依點擊數降冪</summary>
    List<ReportUsageSummary> GetReportSummaries(DateTime since);

    /// <summary>取得啟用中且期間內無任何點擊的報表 (LastUsed 為歷史最後使用時間，null = 從未使用)</summary>
    List<ReportUsageSummary> GetColdReports(DateTime since);

    /// <summary>啟用中報表總數</summary>
    int GetActiveReportCount();

    /// <summary>分頁查詢使用明細 (employeeIds 為 null 表示不篩選使用者)</summary>
    UsageLogPage GetLogs(int? reportId, List<string>? employeeIds, DateTime? from, DateTime? to, int page, int pageSize);
}
