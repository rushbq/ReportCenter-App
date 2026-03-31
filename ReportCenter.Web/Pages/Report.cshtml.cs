using Microsoft.AspNetCore.Mvc.RazorPages;
using ReportCenter.Web.Models;

namespace ReportCenter.Web.Pages;

public class ReportModel : PageModel
{
    public string DeptId { get; set; } = "procurement";
    public string ReportName { get; set; } = "月度採購成本分析";
    public Department Dept { get; set; } = null!;

    public void OnGet(string dept, string name)
    {
        DeptId = dept ?? "procurement";
        ReportName = name ?? "月度採購成本分析";
        Dept = ReportData.Departments.Find(d => d.Id == DeptId) ?? ReportData.Departments[0];
    }
}
