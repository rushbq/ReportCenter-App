namespace ReportCenter.Web.Repositories;

using ReportCenter.Web.Models;

/// <summary>
/// 首頁年度目標戰情 Mock 實作 — 開發環境用，不依賴 usp_Rpt_Home_YtdTarget。
/// 數字比照交付文件示意 (外銷約 20%、台灣約 23%、中國約 27%)，僅供前端排版與切換驗證。
/// 正式/測試環境改註冊 <see cref="SqlHomeDashboardRepository"/>，走真實 SP。
/// </summary>
public class MockHomeDashboardRepository : IHomeDashboardRepository
{
    // 各區塊固定定義：Code、Name、排序、幣別、年度目標
    private static readonly (string Code, string Name, int Sort, string Cur, decimal Target)[] Defs =
    {
        ("EXPORT", "外銷", 1, "USD", 10_000_000m),
        ("TW",     "台灣", 2, "NTD", 82_000_000m),
        ("CN",     "中國", 3, "RMB", 60_000_000m),
    };

    // 各模式的「月結累計 (M)」基準：累計金額、去年同期累計
    private static (decimal Cum, decimal Last) BaseFigure(string mode, string code) => (mode, code) switch
    {
        ("S", "EXPORT") => (1_995_071m, 2_809_960m),   // 20% / YoY -29%
        ("S", "TW")     => (18_570_642m, 17_195_000m),  // 23% / YoY +8%
        ("S", "CN")     => (16_066_623m, 17_463_720m),  // 27% / YoY -8%
        ("O", "EXPORT") => (2_640_500m, 2_450_000m),    // 26% / YoY +8%
        ("O", "TW")     => (20_910_300m, 22_100_000m),  // 25% / YoY -5%
        ("O", "CN")     => (19_880_100m, 18_300_000m),  // 33% / YoY +9%
        _               => (0m, 0m),
    };

    public (List<YtdBlockKpi> Blocks, List<YtdTrendPoint> Trend) GetYtdTarget(
        string mode, string cumulativeType, int? reportYear)
    {
        var year = reportYear ?? DateTime.Today.Year;

        // 截止月 M：月結 = 當月 − 1；即時 = 當月 (與交付文件 2.3 一致)
        var endMonth = cumulativeType == "R" ? DateTime.Today.Month : DateTime.Today.Month - 1;
        if (endMonth < 0) endMonth = 0;

        // 即時累計比月結多算一個月，累計金額依相同步調外推 (月結為基準)
        var baseMonth = DateTime.Today.Month - 1;
        var scale = baseMonth > 0 && endMonth > 0 ? (decimal)endMonth / baseMonth : 0m;

        var blocks = new List<YtdBlockKpi>();
        var trend = new List<YtdTrendPoint>();

        foreach (var d in Defs)
        {
            var (baseCum, baseLast) = BaseFigure(mode, d.Code);
            var cum = decimal.Round(baseCum * scale, 0);
            var last = decimal.Round(baseLast * scale, 0);

            blocks.Add(new YtdBlockKpi
            {
                ReportYear = year,
                EndMonth = endMonth,
                BlockCode = d.Code,
                BlockName = d.Name,
                BlockSort = d.Sort,
                Currency = d.Cur,
                AnnualTarget = d.Target,
                CumAmount = cum,
                LastYearAmount = last,
                AchievementRate = d.Target > 0 ? cum / d.Target : 0m,
                YoYRate = last != 0 ? (cum - last) / Math.Abs(last) : 0m,
            });

            // 逐月累計走勢：1~M 線性遞增至當年累計，分母固定為全年目標
            for (var m = 1; m <= endMonth; m++)
            {
                var monthCum = decimal.Round(cum * m / endMonth, 0);
                trend.Add(new YtdTrendPoint
                {
                    BlockCode = d.Code,
                    BlockName = d.Name,
                    BlockSort = d.Sort,
                    Currency = d.Cur,
                    ReportMonth = m,
                    CumAmount = monthCum,
                    AnnualTarget = d.Target,
                    TrendRate = d.Target > 0 ? monthCum / d.Target : 0m,
                });
            }
        }

        return (blocks, trend);
    }
}
