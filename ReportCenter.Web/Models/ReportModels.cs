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
    public string DeptId { get; set; } = "";
    public string DeptName { get; set; } = "";
    public string CompanyId { get; set; } = "tw";
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
    public int ReportID { get; set; }
    public string Name { get; set; } = "";
    public string Desc { get; set; } = "";
    public string Cat { get; set; } = "";
    public string Updated { get; set; } = "";
    public bool Fav { get; set; }
    public string ReportTool { get; set; } = "Internal";  // Internal / SmartQuery / SSRS
    public string ReportCode { get; set; } = "";           // URL path 或內部路由
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
    public string ReportPath { get; set; } = "";           // 報表資料夾
    public string ReportCode { get; set; } = "";           // URL 或內部路由
    public string SourceName { get; set; } = "";           // SP/函數名稱
    public DateTime CreateDate { get; set; } = DateTime.Now;
    public DateTime? ModifyDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string Remarks { get; set; } = "";              // 報表描述
    public bool UseCompanyParam { get; set; }               // 是否帶公司別參數
    public int? CategoryID { get; set; }
    public string CategoryName { get; set; } = "";         // 顯示用，由 JOIN 帶出
    public List<DeptAssignment> Departments { get; set; } = [];
    public List<string> Dependencies { get; set; } = [];
}

public class ReportCategory
{
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = "";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class DeptAssignment
{
    public int DeptID { get; set; }
    public string DeptName { get; set; } = "";
}

/// <summary>
/// User_Dept 資料表對應 — 目錄管理用的部門清單
/// </summary>
public class CatalogDept
{
    public string Area { get; set; } = "";
    public string DeptID { get; set; } = "";
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
    public List<DeptUsage> DeptUsagesTW { get; set; } = [];
    public List<DeptUsage> DeptUsagesSH { get; set; } = [];
}

public class DeptUsage
{
    public string DeptName { get; set; } = "";
    public List<ReportCatalogItem> Reports { get; set; } = [];
}
