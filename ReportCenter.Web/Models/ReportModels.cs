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
    public string NickName { get; set; } = "";
    public string CompanyId { get; set; } = "tw";
}

public class ResolvedUserField
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string Source { get; set; } = "";
}

public class AuthClaimInfo
{
    public string Type { get; set; } = "";
    public string Value { get; set; } = "";
    public string ValueType { get; set; } = "";
    public string Issuer { get; set; } = "";
    public string OriginalIssuer { get; set; } = "";
}

public class WindowsAuthDebugInfo
{
    public bool IsAuthenticated { get; set; }
    public string AuthenticationType { get; set; } = "";
    public string IdentityName { get; set; } = "";
    public string NormalizedAccountName { get; set; } = "";
    public string NameClaimType { get; set; } = "";
    public string RoleClaimType { get; set; } = "";
    public string PksysLookupAccount { get; set; } = "";
    public bool PksysUserFound { get; set; }
    public string PksysLookupError { get; set; } = "";
    public UserInfo ResolvedUser { get; set; } = new();
    public List<ResolvedUserField> ResolvedFields { get; set; } = [];
    public List<AuthClaimInfo> Claims { get; set; } = [];
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
    public bool UseCompanyParam { get; set; }              // 是否帶公司別參數 (dbs)
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
    public List<ReportCatalogItem> OrphanItems { get; set; } = [];
    public List<DeptUsage> DeptUsagesTW { get; set; } = [];
    public List<DeptUsage> DeptUsagesSH { get; set; } = [];
}

public class DeptUsage
{
    public string DeptName { get; set; } = "";
    public List<ReportCatalogItem> Reports { get; set; } = [];
}

// ─── 權限管理 ───

/// <summary>User_Profile 對應 (PKSYS)</summary>
public class UserProfileItem
{
    public string AccountName { get; set; } = "";   // Account_Name (員工編號)
    public string DisplayName { get; set; } = "";   // Display_Name (姓名)
    public string DeptID { get; set; } = "";
    public string DeptName { get; set; } = "";       // 從 User_Dept JOIN
    public string NickName { get; set; } = "";       // User_Profile.NickName
}

/// <summary>使用者報表權限</summary>
public class UserReportPermission
{
    public int PermissionID { get; set; }
    public string EmployeeId { get; set; } = "";
    public int ReportID { get; set; }
    public string GrantedBy { get; set; } = "";
    public DateTime GrantedDate { get; set; }
    // JOIN 欄位 (顯示用)
    public string ReportName { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public string ReportTool { get; set; } = "";
}

/// <summary>部門含使用者清單 (人員樹節點)</summary>
public class DeptWithUsers
{
    public string Area { get; set; } = "";
    public string DeptID { get; set; } = "";
    public string DeptName { get; set; } = "";
    public List<UserProfileItem> Users { get; set; } = [];
}

/// <summary>分類含報表清單 (報表樹節點)</summary>
public class CategoryWithReports
{
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = "";
    public List<ReportTreeItem> Reports { get; set; } = [];
}

/// <summary>報表樹葉節點</summary>
public class ReportTreeItem
{
    public int ReportID { get; set; }
    public string ReportName { get; set; } = "";
    public string ReportTool { get; set; } = "";
    public bool IsActive { get; set; }
}

// ─── 報表使用記錄 ───

/// <summary>報表使用記錄明細列 (含 JOIN 顯示欄位)</summary>
public class UsageLogItem
{
    public long LogID { get; set; }
    public int ReportID { get; set; }
    public string EmployeeId { get; set; } = "";
    public string CompanyId { get; set; } = "";
    public string Source { get; set; } = "";       // department / pin / favorite / search
    public DateTime ClickedAt { get; set; }
    // JOIN 欄位 (顯示用)
    public string ReportName { get; set; } = "";
    public string UserName { get; set; } = "";     // PKSYS Display_Name (BLL 補齊)
    public string DeptName { get; set; } = "";     // PKSYS DeptName (BLL 補齊)
}

/// <summary>使用明細分頁結果</summary>
public class UsageLogPage
{
    public int Total { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public List<UsageLogItem> Items { get; set; } = [];
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)Total / PageSize) : 0;
}

/// <summary>使用明細查詢條件</summary>
public class UsageLogQuery
{
    public int? ReportId { get; set; }
    public string? UserKeyword { get; set; }       // 員工編號或姓名模糊查詢
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>單一報表的使用彙總 (期間點擊數、使用人數、最後使用時間)</summary>
public class ReportUsageSummary
{
    public int ReportID { get; set; }
    public string ReportName { get; set; } = "";
    public string ReportTool { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public int Clicks { get; set; }
    public int Users { get; set; }
    public DateTime? LastUsed { get; set; }        // null = 從未被使用
}

/// <summary>單日點擊數 (趨勢圖用)</summary>
public class DailyUsageCount
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}

/// <summary>使用分析總覽 ViewModel (/Admin/Usage Tab1)</summary>
public class UsageOverview
{
    public int Clicks30 { get; set; }              // 近 30 日總點擊
    public int ActiveUsers30 { get; set; }         // 近 30 日活躍使用者數
    public int UsedReports30 { get; set; }         // 近 30 日有被使用的報表數
    public int ActiveReportTotal { get; set; }     // 啟用中報表總數
    public List<DailyUsageCount> Daily30 { get; set; } = [];
    public List<ReportUsageSummary> Top30 { get; set; } = [];
    public List<ReportUsageSummary> Top90 { get; set; } = [];
    public List<ReportUsageSummary> ColdReports { get; set; } = [];  // 啟用中且 90 日內 0 次
}
