namespace ReportCenter.Web.Models.Settings;

/// <summary>
/// 開發環境模擬 Windows AD 身份的設定 — 對應 appsettings.Development.json 中的 MockWindowsAuth 區段
/// </summary>
public class MockWindowsAuthSettings
{
    public const string SectionName = "MockWindowsAuth";

    /// <summary>Windows 帳號名稱，格式 DOMAIN\username</summary>
    public string UserName { get; set; } = "";

    /// <summary>顯示名稱（中文姓名）</summary>
    public string DisplayName { get; set; } = "";

    /// <summary>員工編號</summary>
    public string EmployeeId { get; set; } = "";

    /// <summary>部門名稱</summary>
    public string Department { get; set; } = "";

    /// <summary>部門代碼</summary>
    public string DepartmentId { get; set; } = "";
}
