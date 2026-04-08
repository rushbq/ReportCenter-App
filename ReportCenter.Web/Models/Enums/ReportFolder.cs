namespace ReportCenter.Web.Models.Enums;

/// <summary>
/// 報表資料夾 — 對應報表目錄中的資料夾分類
/// </summary>
public enum ReportFolder
{
    [System.ComponentModel.Description("00-經營層報表")]
    Management = 1,

    [System.ComponentModel.Description("01-總經理室")]
    GeneralManager = 2,

    [System.ComponentModel.Description("20-外業報表")]
    ExternalSales = 20,
    
    [System.ComponentModel.Description("30-內業報表")]
    InternalSales = 30,

    [System.ComponentModel.Description("50-上海業務")]
    ShSales = 50,

    [System.ComponentModel.Description("80-會計報表")]
    Finances = 80
}

public static class ReportFolderExtensions
{
    private static readonly Dictionary<ReportFolder, string> _labels = new()
    {
        [ReportFolder.Management]     = "00-經營層報表",
        [ReportFolder.GeneralManager] = "01-總經理室",
        [ReportFolder.ExternalSales]  = "20-外業報表",
        [ReportFolder.InternalSales]  = "30-內業報表",
        [ReportFolder.ShSales]        = "50-上海業務",
        [ReportFolder.Finances]       = "80-會計報表",
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
