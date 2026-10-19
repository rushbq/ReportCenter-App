namespace ReportCenter.Web.Services;

using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using ReportCenter.Web.Models;
using ReportCenter.Web.Repositories;

/// <summary>
/// 首頁年度目標戰情 BLL 實作 — 正規化輸入參數並組裝 ViewModel。
/// 年度固定系統當年度 (傳 NULL 由 SP 判斷)，首頁不開放切歷史/未來年度。
/// </summary>
/// <remarks>
/// 首頁是全員入口，每次載入都執行全年度 YTD SP 成本高，故以 IMemoryCache 快取：
/// 月結累計 (M) 已結帳、當天不會變 → 快取到當天結束；
/// 即時累計 (R) 含當月未結帳數字 → 快取 <see cref="RealtimeCacheMinutes"/> 分鐘。
/// 快取 key 含日期，跨日自動失效；結果不分使用者 (首頁戰情不套權限、不分公司別)。
/// </remarks>
public class HomeDashboardService : IHomeDashboardService
{
    /// <summary>即時累計的快取分鐘數</summary>
    private const int RealtimeCacheMinutes = 5;

    private readonly IHomeDashboardRepository _repo;
    private readonly IMemoryCache _cache;
    private readonly ILogger<HomeDashboardService> _logger;

    public HomeDashboardService(IHomeDashboardRepository repo, IMemoryCache cache, ILogger<HomeDashboardService> logger)
    {
        _repo = repo;
        _cache = cache;
        _logger = logger;
    }

    public HomeYtdDashboard GetYtdTarget(string? mode, string? cumulativeType)
    {
        var m = NormalizeMode(mode);
        var c = NormalizeCumType(cumulativeType);
        var today = DateTime.Today;

        return _cache.GetOrCreate($"HomeYtd:{m}:{c}:{today:yyyyMMdd}", entry =>
        {
            entry.AbsoluteExpiration = c == "M"
                ? today.AddDays(1)
                : DateTimeOffset.Now.AddMinutes(RealtimeCacheMinutes);

            return Load(m, c);
        })!;
    }

    private HomeYtdDashboard Load(string mode, string cumType)
    {
        var sw = Stopwatch.StartNew();
        var (blocks, trend) = _repo.GetYtdTarget(mode, cumType, reportYear: null);
        _logger.LogInformation("首頁戰情快取未命中，重新計算 Mode={Mode} CumulativeType={CumulativeType} 耗時 {ElapsedMs} ms",
            mode, cumType, sw.ElapsedMilliseconds);

        // 三區塊固定同年度、同截止月，取任一列回填 meta；無資料時退回系統當年度
        var meta = blocks.FirstOrDefault();
        return new HomeYtdDashboard
        {
            Mode = mode,
            CumulativeType = cumType,
            ReportYear = meta?.ReportYear ?? DateTime.Today.Year,
            EndMonth = meta?.EndMonth ?? 0,
            Blocks = blocks,
            Trend = trend,
        };
    }

    private static string NormalizeMode(string? mode) =>
        string.Equals(mode, "O", StringComparison.OrdinalIgnoreCase) ? "O" : "S";

    private static string NormalizeCumType(string? cumType) =>
        string.Equals(cumType, "R", StringComparison.OrdinalIgnoreCase) ? "R" : "M";
}
