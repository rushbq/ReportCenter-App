using Microsoft.AspNetCore.Mvc.RazorPages;
using ReportCenter.Web.Models;
using ReportCenter.Web.Services;

namespace ReportCenter.Web.Pages;

public class ReportModel : PageModel
{
    private readonly IReportService _svc;

    public ReportModel(IReportService svc) => _svc = svc;

    public string DeptId { get; set; } = "procurement";
    public string ReportName { get; set; } = "月度採購成本分析";
    public Department Dept { get; set; } = null!;
    public List<MaterialRow> MaterialRows { get; set; } = [];
    public int TotalRows { get; set; }
    public int CurrentPage { get; set; } = 1;

    public void OnGet(string dept, string name, int page = 1)
    {
        DeptId = dept ?? "procurement";
        ReportName = name ?? "月度採購成本分析";
        CurrentPage = page < 1 ? 1 : page;
        Dept = _svc.GetDepartment(DeptId) ?? _svc.GetDepartments()[0];
        MaterialRows = _svc.GetMaterialRows(DeptId, ReportName, CurrentPage);
        TotalRows = _svc.GetMaterialRowCount(DeptId, ReportName);
    }
}
