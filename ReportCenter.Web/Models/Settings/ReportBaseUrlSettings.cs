namespace ReportCenter.Web.Models.Settings;

/// <summary>
/// 報表外部連結的 BaseUrl 設定 — 對應 appsettings.json 中的 ReportBaseUrls 區段
/// </summary>
public class ReportBaseUrlSettings
{
    public const string SectionName = "ReportBaseUrls";

    public string SmartQuery { get; set; } = "";
    public string SSRS { get; set; } = "";
}
