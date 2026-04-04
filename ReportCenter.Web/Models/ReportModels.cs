namespace ReportCenter.Web.Models;

// ─── 基礎模型 ───

public class Company
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string ShortName { get; set; } = "";
    public string Region { get; set; } = "";
}

public class UserInfo
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Initials { get; set; } = "";
    public string DeptId { get; set; } = "";
    public string DeptName { get; set; } = "";
    public string Title { get; set; } = "";
    public string CompanyId { get; set; } = "";
}

public class Department
{
    public string Id { get; set; } = "";
    public string Label { get; set; } = "";
    public string Icon { get; set; } = "";
    public int Count { get; set; }
    public List<string> Subs { get; set; } = [];
}

public class Report
{
    public string Name { get; set; } = "";
    public string Desc { get; set; } = "";
    public string Cat { get; set; } = "";
    public string Updated { get; set; } = "";
    public bool Fav { get; set; }
}

public class QuickAccess
{
    public string Dept { get; set; } = "";
    public string DeptId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Tag { get; set; } = "";
}

public class MaterialRow
{
    public string Material { get; set; } = "";
    public string Supplier { get; set; } = "";
    public string Qty { get; set; } = "";
    public string UnitPrice { get; set; } = "";
    public string Amount { get; set; } = "";
    public double Change { get; set; }
}

// ─── 儀表板 KPI ───

public class KpiItem
{
    public string Title { get; set; } = "";
    public string Value { get; set; } = "";
    public double Trend { get; set; }
    public string Note { get; set; } = "";
}

public class DashboardKpi
{
    public List<KpiItem> Items { get; set; } = [];
    public string LastUpdated { get; set; } = "";
}

// ─── 圖表資料 ───

public class ChartDataset
{
    public string Label { get; set; } = "";
    public List<double> Data { get; set; } = [];
    public string Color { get; set; } = "";
    public string Type { get; set; } = "line"; // line, bar, doughnut
    public bool Fill { get; set; }
    public string? BorderDash { get; set; }
}

public class ChartData
{
    public List<string> Labels { get; set; } = [];
    public List<ChartDataset> Datasets { get; set; } = [];
}
