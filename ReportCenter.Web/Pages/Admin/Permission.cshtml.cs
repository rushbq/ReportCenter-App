using Microsoft.AspNetCore.Mvc;
using ReportCenter.Web.Models;
using ReportCenter.Web.Services;

namespace ReportCenter.Web.Pages.Admin;

/// <summary>
/// 權限管理頁 — 提供報表權限的批次指派與個人權限管理。
/// 雙模式：功能→多人 (批次) / 單一人→功能 (個人)。
/// 存取控制 (含 POST/AJAX 端點) 由 AdminPageModel 統一把關。
/// </summary>
public class PermissionModel : AdminPageModel
{
    private readonly IPermissionService _permSvc;

    public PermissionModel(IPermissionService permSvc, IReportService reportSvc)
        : base(reportSvc, permSvc)
    {
        _permSvc = permSvc;
    }

    /// <summary>分類→報表樹 (Mode A 報表選擇)</summary>
    public List<CategoryWithReports> ReportTree { get; set; } = [];

    /// <summary>部門→使用者樹 (Mode A 人員選擇)</summary>
    public List<DeptWithUsers> UserTree { get; set; } = [];

    public IActionResult OnGet()
    {
        ReportTree = _permSvc.GetReportTree();
        UserTree = _permSvc.GetDeptUserTree();
        return Page();
    }

    /// <summary>批次授權 (Mode A)</summary>
    public IActionResult OnPostGrant(string employeeIds, string reportIds)
    {
        var empList = ParseCsv(employeeIds);
        var rptList = ParseIntCsv(reportIds);
        if (empList.Count == 0 || rptList.Count == 0)
            return new JsonResult(new { success = false, message = "請選擇使用者與報表" });

        var inserted = _permSvc.GrantBatch(empList, rptList, CurrentUserId);
        var total = empList.Count * rptList.Count;
        var skipped = total - inserted;

        return new JsonResult(new { success = true, inserted, skipped, total });
    }

    /// <summary>取得個人權限清單 (Mode B)</summary>
    public IActionResult OnGetUserPermissions(string empId)
    {
        if (string.IsNullOrEmpty(empId))
            return new JsonResult(new { success = false, message = "缺少使用者編號" });

        var perms = _permSvc.GetUserPermissions(empId);
        return new JsonResult(new { success = true, permissions = perms });
    }

    /// <summary>撤銷單一權限 (Mode B)</summary>
    public IActionResult OnPostRevoke(string empId, int reportId)
    {
        if (string.IsNullOrEmpty(empId))
            return new JsonResult(new { success = false, message = "缺少使用者編號" });

        var result = _permSvc.RevokePermission(empId, reportId);
        return new JsonResult(new { success = result });
    }

    /// <summary>搜尋使用者 (Mode B)</summary>
    public IActionResult OnGetSearchUsers(string keyword)
    {
        var users = _permSvc.SearchUsers(keyword);
        return new JsonResult(new { success = true, users });
    }

    /// <summary>單人批次新增權限 (Mode B)</summary>
    public IActionResult OnPostGrantSingle(string empId, string reportIds)
    {
        if (string.IsNullOrEmpty(empId))
            return new JsonResult(new { success = false, message = "缺少使用者編號" });

        var rptList = ParseIntCsv(reportIds);
        if (rptList.Count == 0)
            return new JsonResult(new { success = false, message = "請選擇報表" });

        var inserted = _permSvc.GrantBatch([empId], rptList, CurrentUserId);

        return new JsonResult(new { success = true, inserted });
    }

    private static List<string> ParseCsv(string? csv)
        => string.IsNullOrEmpty(csv) ? [] : csv.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

    private static List<int> ParseIntCsv(string? csv)
        => string.IsNullOrEmpty(csv) ? [] : csv.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Where(s => int.TryParse(s, out _)).Select(int.Parse).ToList();
}
