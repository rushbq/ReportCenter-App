namespace ReportCenter.Web.Models.Settings;

/// <summary>
/// Windows 驗證診斷頁設定 — 對應 appsettings.json 中的 WindowsAuthDebug 區段
/// </summary>
public class WindowsAuthDebugSettings
{
    public const string SectionName = "WindowsAuthDebug";

    /// <summary>是否啟用 Windows 驗證診斷頁</summary>
    public bool Enabled { get; set; }
}
