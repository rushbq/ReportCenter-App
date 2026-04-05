using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReportCenter.Web.Models;
using ReportCenter.Web.Services;

namespace ReportCenter.Web.Pages.Admin;

public class CatalogModel : PageModel
{
    private readonly IReportService _svc;

    public CatalogModel(IReportService svc) => _svc = svc;

    public List<ReportCatalogItem> CatalogItems { get; set; } = [];
    public List<Department> Departments { get; set; } = [];
    public List<string> AllDependencies { get; set; } = [];
    public List<ReportCategory> Categories { get; set; } = [];
    public List<CatalogDept> CatalogDepts { get; set; } = [];

    public void OnGet(string? tool, string? active, string? search)
    {
        bool? isActive = active switch
        {
            "true" => true,
            "false" => false,
            _ => null
        };
        CatalogItems = _svc.GetCatalogItems(tool, isActive, search);
        Departments = _svc.GetDepartments();
        AllDependencies = _svc.GetAllDependencyObjects();
        Categories = _svc.GetCategories();
        CatalogDepts = _svc.GetCatalogDepartments();
    }

    public IActionResult OnPostSave(
        int reportId, string reportName, string reportTool, string reportPath,
        string reportCode, string sourceName, bool isActive, string? remarks,
        string? deptIds, string? dependencies, int? categoryId)
    {
        var item = reportId > 0 ? _svc.GetCatalogItem(reportId) ?? new() : new();
        item.ReportID = reportId;
        item.ReportName = reportName ?? "";
        item.ReportTool = reportTool ?? "Internal";
        item.ReportPath = reportPath ?? "";
        item.ReportCode = reportCode ?? "";
        item.SourceName = sourceName ?? "";
        item.IsActive = isActive;
        item.Remarks = remarks ?? "";
        item.CategoryID = categoryId;

        // 解析部門
        item.Departments = [];
        if (!string.IsNullOrEmpty(deptIds))
        {
            foreach (var pair in deptIds.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = pair.Split('|');
                if (parts.Length == 2 && int.TryParse(parts[0], out var deptIdVal))
                    item.Departments.Add(new DeptAssignment { DeptID = deptIdVal, DeptName = parts[1] });
            }
        }

        // 解析相依物件
        item.Dependencies = string.IsNullOrEmpty(dependencies)
            ? []
            : dependencies.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

        _svc.SaveCatalogItem(item);
        return RedirectToPage(new { msg = "saved" });
    }

    public IActionResult OnPostDelete(int reportId)
    {
        _svc.DeleteCatalogItem(reportId);
        return RedirectToPage(new { msg = "deleted" });
    }

    public IActionResult OnPostToggle(int reportId)
    {
        var item = _svc.GetCatalogItem(reportId);
        if (item != null)
        {
            item.IsActive = !item.IsActive;
            _svc.SaveCatalogItem(item);
        }
        return RedirectToPage(new { msg = "toggled" });
    }
}
