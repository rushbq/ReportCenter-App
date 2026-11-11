namespace ReportCenter.Web.Services;

using System.Globalization;
using ReportCenter.Web.Models;

/// <summary>
/// 報表顯示順序的唯一規則：自訂排序 (SortOrder) 由小到大 → 報表名稱遞增。
/// 部門頁、收藏頁、部門使用矩陣共用，避免各處各自排序而不一致。
/// </summary>
public static class ReportDisplayOrder
{
    /// <summary>報表名稱比較採 zh-TW 文化 (筆劃序)，與 SQL Server 中文定序的結果相近</summary>
    private static readonly StringComparer NameComparer =
        StringComparer.Create(CultureInfo.GetCultureInfo("zh-TW"), ignoreCase: true);

    /// <summary>依自訂排序 → 報表名稱排列</summary>
    public static List<ReportCatalogItem> Sort(IEnumerable<ReportCatalogItem> items)
        => items.OrderBy(i => i.SortOrder).ThenBy(i => i.ReportName, NameComparer).ToList();
}
