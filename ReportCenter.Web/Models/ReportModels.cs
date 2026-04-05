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
    public string Category { get; set; } = "";
    public string Period { get; set; } = "";
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

// ─── 報表目錄管理 ───

public class ReportCatalogItem
{
    public int ReportID { get; set; }
    public string ReportName { get; set; } = "";
    public string ReportTool { get; set; } = "Internal";  // Internal / SmartQuery / SSRS
    public string ReportPath { get; set; } = "";           // 部門路徑
    public string ReportCode { get; set; } = "";           // URL 或內部路由
    public string SourceName { get; set; } = "";           // SP/函數名稱
    public DateTime CreateDate { get; set; } = DateTime.Now;
    public DateTime? ModifyDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string Remarks { get; set; } = "";
    public List<DeptAssignment> Departments { get; set; } = [];
    public List<string> Dependencies { get; set; } = [];
}

public class DeptAssignment
{
    public int DeptID { get; set; }
    public string DeptName { get; set; } = "";
}

public class AdminStats
{
    public int TotalReports { get; set; }
    public int ActiveReports { get; set; }
    public int InactiveReports { get; set; }
    public int InternalCount { get; set; }
    public int SmartQueryCount { get; set; }
    public int SsrsCount { get; set; }
    public int OrphanCount { get; set; }
    public List<ReportCatalogItem> RecentItems { get; set; } = [];
    public List<ReportCatalogItem> OrphanItems { get; set; } = [];
    public List<DeptUsage> DeptUsages { get; set; } = [];
}

public class DeptUsage
{
    public string DeptName { get; set; } = "";
    public List<ReportCatalogItem> Reports { get; set; } = [];
}
