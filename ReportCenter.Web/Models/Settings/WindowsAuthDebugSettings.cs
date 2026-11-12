namespace ReportCenter.Web.Models.Settings;

/// <summary>
/// 登入身分診斷頁 (/Debug/WindowsAuth) 設定 — 對應 appsettings.json 中的 WindowsAuthDebug 區段
/// </summary>
public class WindowsAuthDebugSettings
{
    public const string SectionName = "WindowsAuthDebug";

    /// <summary>是否啟用登入身分診斷頁</summary>
    public bool Enabled { get; set; }
}
