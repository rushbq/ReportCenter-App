namespace ReportCenter.Web.Models.Settings;

/// <summary>
/// 各公司別要顯示的部門設定 — 由 appsettings.json 的 DepartmentDisplay 區段綁定
/// </summary>
public class DepartmentDisplaySettings
{
    public const string SectionName = "DepartmentDisplay";
    public Dictionary<string, RegionDeptConfig> Regions { get; set; } = new();
}

public class RegionDeptConfig
{
    public List<DeptEntry> Depts { get; set; } = [];
}

public class DeptEntry
{
    public string DeptID { get; set; } = "";
    public string Icon { get; set; } = "folder";  // Lucide icon name
}
