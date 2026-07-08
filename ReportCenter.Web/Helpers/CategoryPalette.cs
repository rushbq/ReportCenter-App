namespace ReportCenter.Web.Helpers;

/// <summary>
/// 報表分類的配色對照。依分類名稱決定性地對應到 5 種色票之一，
/// 讓同一分類在全站（卡片、列表、分頁）呈現一致的顏色，兼具分類辨識與視覺層次。
/// 雜湊邏輯須與前端 site.js 的 window.catBadge 保持一致 (字元碼加總取餘數)。
/// </summary>
public static class CategoryPalette
{
    // 5 種柔和且跨色相分佈的色票 (藍/琥珀/玫瑰/紫/石板灰)，刻意避開品牌青綠 (pri)
    // 以免與選取狀態混淆。依序指派可讓相鄰分類色差最大。
    private static readonly (string Badge, string Dot)[] Palette =
    [
        ("bg-sky-50 text-sky-700",       "bg-sky-400"),
        ("bg-amber-50 text-amber-700",   "bg-amber-400"),
        ("bg-rose-50 text-rose-700",     "bg-rose-400"),
        ("bg-violet-50 text-violet-700", "bg-violet-400"),
        ("bg-slate-100 text-slate-600",  "bg-slate-400"),
    ];

    private static int Wrap(int index) => ((index % Palette.Length) + Palette.Length) % Palette.Length;

    /// <summary>依「分類在部門內的序位」取得徽章色 (相鄰分類色差最大，保證同一畫面可辨識)。</summary>
    public static string BadgeByIndex(int index) => Palette[Wrap(index)].Badge;

    /// <summary>依序位取得小圓點色 (用於分頁標籤前的識別點)。</summary>
    public static string DotByIndex(int index) => Palette[Wrap(index)].Dot;
}
