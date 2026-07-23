namespace ReportCenter.Web.Services;

using ReportCenter.Web.Models;
using ReportCenter.Web.Repositories;

/// <summary>
/// 首頁年度目標戰情 BLL 實作 — 正規化輸入參數並組裝 ViewModel。
/// 年度固定系統當年度 (傳 NULL 由 SP 判斷)，首頁不開放切歷史/未來年度。
/// </summary>
public class HomeDashboardService : IHomeDashboardService
{
    private readonly IHomeDashboardRepository _repo;

    public HomeDashboardService(IHomeDashboardRepository repo) => _repo = repo;

    public HomeYtdDashboard GetYtdTarget(string? mode, string? cumulativeType)
    {
        var m = NormalizeMode(mode);
        var c = NormalizeCumType(cumulativeType);

        var (blocks, trend) = _repo.GetYtdTarget(m, c, reportYear: null);

        // 三區塊固定同年度、同截止月，取任一列回填 meta；無資料時退回系統當年度
        var meta = blocks.FirstOrDefault();
        return new HomeYtdDashboard
        {
            Mode = m,
            CumulativeType = c,
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
