using Microsoft.AspNetCore.Mvc;
using ReportCenter.Web.Models;
using ReportCenter.Web.Models.Enums;
using ReportCenter.Web.Models.Settings;
using ReportCenter.Web.Services;

namespace ReportCenter.Web.Pages.Admin;

/// <summary>
/// 報表目錄管理頁 — 提供報表清冊的 CRUD、篩選、啟停用操作。
/// 職責：組裝前端所需的參考資料 (部門、分類、資料夾等)，
/// 並透過 POST handler 委派 ICatalogService 處理寫入邏輯。
/// 存取控制 (含 POST 寫入端點) 由 AdminPageModel 統一把關。
/// </summary>
public class CatalogModel : AdminPageModel
{
    private readonly ICatalogService _catalogSvc;
    private readonly IUsageService _usageSvc;

    public CatalogModel(ICatalogService catalogSvc, IUsageService usageSvc,
        IPermissionService permSvc, IReportService reportSvc)
        : base(reportSvc, permSvc)
    {
        _catalogSvc = catalogSvc;
        _usageSvc = usageSvc;
    }

    /// <summary>目錄管理可建立的報表工具類型 (Internal 已停止開發)</summary>
    private static readonly HashSet<string> AllowedReportTools = ["SmartQuery", "SSRS"];

    // ─── 頁面資料屬性 ───

    /// <summary>報表清冊清單 (可依篩選條件過濾)</summary>
    public List<ReportCatalogItem> CatalogItems { get; set; } = [];

    /// <summary>所有已知的相依物件名稱 (供 autocomplete)</summary>
    public List<string> AllDependencies { get; set; } = [];

    /// <summary>報表分類清單</summary>
    public List<ReportCategory> Categories { get; set; } = [];

    /// <summary>所有部門合併清單 (供 deptIdsString 查找用)</summary>
    public List<CatalogDept> CatalogDepts { get; set; } = [];

    /// <summary>報表資料夾列舉</summary>
    public IReadOnlyList<(ReportFolder Value, string Label)> ReportFolders { get; set; } = [];

    /// <summary>外部報表 BaseUrl 設定 (SmartQuery / SSRS)</summary>
    public ReportBaseUrlSettings BaseUrls { get; set; } = new();

    /// <summary>台灣部門清單 (依 User_Dept Area=TW)</summary>
    public List<CatalogDept> AllDeptsTW { get; set; } = [];

    /// <summary>上海部門清單 (依 User_Dept Area=SH)</summary>
    public List<CatalogDept> AllDeptsCN { get; set; } = [];

    /// <summary>近 30 日使用彙總 (ReportID → 彙總)；清單中查無 key 者表示期間內 0 次</summary>
    public Dictionary<int, ReportUsageSummary> UsageMap { get; set; } = [];

    // ─── GET Handler ───

    public IActionResult OnGet(string? tool, string? active, string? search)
    {
        bool? isActive = active switch
        {
            "true" => true,
            "false" => false,
            _ => null
        };

        CatalogItems = _catalogSvc.GetCatalogItems(tool, isActive, search);
        UsageMap = _usageSvc.GetRecentUsageMap();
        AllDependencies = _catalogSvc.GetAllDependencyObjects();
        Categories = _catalogSvc.GetCategories();

        // 載入所有部門（不分公司），依區域分組供 Modal 部門指派使用
        AllDeptsTW = _catalogSvc.GetCatalogDepartments("tw");
        AllDeptsCN = _catalogSvc.GetCatalogDepartments("sh");
        CatalogDepts = [.. AllDeptsTW, .. AllDeptsCN];

        ReportFolders = _catalogSvc.GetReportFolders();
        BaseUrls = _catalogSvc.GetBaseUrls();
        return Page();
    }

    // ─── POST Handlers ───

    /// <summary>儲存 (新增/更新) 報表目錄項目</summary>
    public IActionResult OnPostSave(
        int reportId, string reportName, string reportTool, string reportPath,
        string reportCode, string sourceName, bool isActive, string? remarks,
        bool useCompanyParam, string? deptIds, string? dependencies, int? categoryId)
    {
        // 後端驗證 (不可只依賴前端)：名稱、代碼必填，工具類型限目錄可建立的類型
        if (string.IsNullOrWhiteSpace(reportName) || string.IsNullOrWhiteSpace(reportCode)
            || !AllowedReportTools.Contains(reportTool ?? ""))
            return RedirectToPage(new { msg = "invalid" });

        var item = reportId > 0 ? _catalogSvc.GetCatalogItem(reportId) ?? new() : new();
        item.ReportID = reportId;
        item.ReportName = reportName.Trim();
        item.ReportTool = reportTool!;
        item.ReportPath = reportPath ?? "";
        item.ReportCode = reportCode.Trim();
        item.SourceName = sourceName ?? "";
        item.IsActive = isActive;
        item.Remarks = remarks ?? "";
        item.UseCompanyParam = useCompanyParam;
        item.CategoryID = categoryId;
        item.Departments = ParseDepartments(deptIds);
        item.Dependencies = ParseDependencies(dependencies);

        var saved = _catalogSvc.SaveCatalogItem(item);
        // focus：前端據此捲動並標亮剛儲存的那一列 (新增時才知道新 ID)
        return RedirectToPage(new { msg = "saved", focus = saved.ReportID });
    }

    /// <summary>刪除報表目錄項目</summary>
    public IActionResult OnPostDelete(int reportId)
    {
        _catalogSvc.DeleteCatalogItem(reportId);
        return RedirectToPage(new { msg = "deleted" });
    }

    /// <summary>切換報表啟停用狀態 (AJAX)；回傳切換後狀態與更新日期供前端就地更新該列</summary>
    public IActionResult OnPostToggle(int reportId)
    {
        if (!_catalogSvc.ToggleCatalogItem(reportId))
            return new JsonResult(new { success = false, message = "找不到此報表" });

        var item = _catalogSvc.GetCatalogItem(reportId)!;
        return new JsonResult(new
        {
            success = true,
            isActive = item.IsActive,
            displayDate = (item.ModifyDate ?? item.CreateDate).ToString("yyyy/MM/dd")
        });
    }

    // ─── 私有解析方法 ───

    /// <summary>
    /// 解析前端傳入的部門指派字串。
    /// 格式: "deptID|deptName,deptID|deptName,..."
    /// </summary>
    private static List<DeptAssignment> ParseDepartments(string? deptIds)
    {
        if (string.IsNullOrEmpty(deptIds)) return [];

        var result = new List<DeptAssignment>();
        foreach (var pair in deptIds.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('|');
            if (parts.Length == 2 && int.TryParse(parts[0], out var deptIdVal))
                result.Add(new DeptAssignment { DeptID = deptIdVal, DeptName = parts[1] });
        }
        return result;
    }

    /// <summary>
    /// 解析前端傳入的相依物件字串。
    /// 格式: "objectName1,objectName2,..."
    /// </summary>
    private static List<string> ParseDependencies(string? dependencies)
    {
        if (string.IsNullOrEmpty(dependencies)) return [];
        return dependencies.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
    }
}
