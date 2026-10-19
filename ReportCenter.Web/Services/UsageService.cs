namespace ReportCenter.Web.Services;

using ReportCenter.Web.Models;
using ReportCenter.Web.Repositories;

/// <summary>
/// 報表使用記錄 BLL 實作 — 防重複點擊記錄、使用分析統計
/// </summary>
public class UsageService : IUsageService
{
    /// <summary>同一使用者對同一報表的防重複時間窗 (秒)</summary>
    public const int DedupeSeconds = 30;

    /// <summary>近期統計期間 (天)</summary>
    private const int RecentDays = 30;

    /// <summary>冷門判定期間 (天)</summary>
    private const int ColdDays = 90;

    /// <summary>合法的來源入口值 (前端 data-report-source)</summary>
    private static readonly HashSet<string> ValidSources = ["department", "pin", "favorite", "search"];

    private readonly IUsageRepository _repo;
    private readonly IPksysRepository _pksys;
    private readonly IReportService _reportSvc;

    public UsageService(
        IUsageRepository repo,
        IPksysRepository pksys,
        IReportService reportSvc)
    {
        _repo = repo;
        _pksys = pksys;
        _reportSvc = reportSvc;
    }

    public bool TrackClick(int reportId, string? source)
    {
        if (reportId <= 0) return false;

        var user = _reportSvc.GetCurrentUser();
        if (string.IsNullOrEmpty(user.Id)) return false;

        var safeSource = source != null && ValidSources.Contains(source) ? source : "";

        // 公司別與頂欄顯示、報表連結共用同一套判定 (IReportService.GetCurrentCompanyId)
        return _repo.TryInsert(reportId, user.Id, _reportSvc.GetCurrentCompanyId(), safeSource, DedupeSeconds);
    }

    public UsageOverview GetOverview()
    {
        // 近 N 日以「含今天的滾動天數」計算 (30 日 = 今天往前推 29 天的 00:00 起)
        var since30 = DateTime.Today.AddDays(-(RecentDays - 1));
        var since90 = DateTime.Today.AddDays(-(ColdDays - 1));

        var (clicks30, users30) = _repo.GetTotals(since30);
        var summaries30 = _repo.GetReportSummaries(since30);

        return new UsageOverview
        {
            Clicks30 = clicks30,
            ActiveUsers30 = users30,
            UsedReports30 = summaries30.Count,
            ActiveReportTotal = _repo.GetActiveReportCount(),
            Daily30 = FillMissingDays(_repo.GetDailyCounts(since30), since30),
            Top30 = summaries30.Take(10).ToList(),
            Top90 = _repo.GetReportSummaries(since90).Take(10).ToList(),
            ColdReports = _repo.GetColdReports(since90)
        };
    }

    public UsageLogPage GetLogs(UsageLogQuery query)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        // 使用者關鍵字 → PKSYS 模糊比對出員編清單再進 SQL 篩選
        List<string>? employeeIds = null;
        if (!string.IsNullOrWhiteSpace(query.UserKeyword))
        {
            employeeIds = _pksys.SearchUsers(query.UserKeyword.Trim())
                .Select(u => u.AccountName).ToList();
            if (employeeIds.Count == 0)
                return new UsageLogPage { Page = page, PageSize = pageSize };
        }

        // 結束日含當天 (加一天後用 < 比較)
        var to = query.To?.Date.AddDays(1);
        var result = _repo.GetLogs(query.ReportId, employeeIds, query.From?.Date, to, page, pageSize);

        FillUserNames(result.Items);
        return result;
    }

    public List<ReportUsageSummary> GetColdReports()
        => _repo.GetColdReports(DateTime.Today.AddDays(-(ColdDays - 1)));

    public Dictionary<int, ReportUsageSummary> GetRecentUsageMap()
        => _repo.GetReportSummaries(DateTime.Today.AddDays(-(RecentDays - 1)))
                .ToDictionary(s => s.ReportID);

    /// <summary>補齊明細列的使用者姓名與部門 (PKSYS)；查無者以員編顯示</summary>
    private void FillUserNames(List<UsageLogItem> items)
    {
        if (items.Count == 0) return;

        // 只查本頁出現的員編 (每頁最多 100 筆)，不再載入全部在職人員
        var userMap = _pksys.GetUsersByAccounts(items.Select(i => i.EmployeeId))
            .GroupBy(u => u.AccountName)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var item in items)
        {
            if (userMap.TryGetValue(item.EmployeeId, out var user))
            {
                item.UserName = user.DisplayName;
                item.DeptName = user.DeptName;
            }
            else
            {
                item.UserName = item.EmployeeId;   // 離職或查無：顯示員編
            }
        }
    }

    /// <summary>趨勢圖用：把沒有點擊的日期補 0，保證回傳連續日期序列</summary>
    private static List<DailyUsageCount> FillMissingDays(List<DailyUsageCount> raw, DateTime since)
    {
        var map = raw.ToDictionary(d => d.Date.Date, d => d.Count);
        var result = new List<DailyUsageCount>();
        for (var day = since.Date; day <= DateTime.Today; day = day.AddDays(1))
            result.Add(new DailyUsageCount { Date = day, Count = map.GetValueOrDefault(day) });
        return result;
    }
}
