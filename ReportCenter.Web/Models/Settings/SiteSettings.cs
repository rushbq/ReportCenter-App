namespace ReportCenter.Web.Models.Settings;

/// <summary>
/// 網站基本資訊設定 — 對應 appsettings.json 中的 Site 區段
/// </summary>
public class SiteSettings
{
    public const string SectionName = "Site";

    /// <summary>瀏覽器分頁標題 (&lt;title&gt;)</summary>
    public string Title { get; set; } = "ReportCenter";
}
