namespace ReportCenter.Web.Models.Enums;

/// <summary>
/// 報表資料夾 — 對應報表目錄中的資料夾分類
/// </summary>
public enum ReportFolder
{
    [System.ComponentModel.Description("01-總經理室")]
    GeneralManager = 1,

    [System.ComponentModel.Description("02-業績報表")]
    SalesReport = 2,

    [System.ComponentModel.Description("03-經營層")]
    Management = 3,

    [System.ComponentModel.Description("20-外業部")]
    ExternalSales = 20
}

public static class ReportFolderExtensions
{
    private static readonly Dictionary<ReportFolder, string> _labels = new()
    {
        [ReportFolder.GeneralManager] = "01-總經理室",
        [ReportFolder.SalesReport] = "02-業績報表",
        [ReportFolder.Management] = "03-經營層",
        [ReportFolder.ExternalSales] = "20-外業部",
    };

    public static string ToLabel(this ReportFolder folder)
        => _labels.TryGetValue(folder, out var label) ? label : folder.ToString();

    public static ReportFolder? FromLabel(string? label)
    {
        if (string.IsNullOrEmpty(label)) return null;
        foreach (var kv in _labels)
            if (kv.Value == label) return kv.Key;
        return null;
    }

    public static IReadOnlyList<(ReportFolder Value, string Label)> All()
        => _labels.Select(kv => (kv.Key, kv.Value)).ToList();
}
