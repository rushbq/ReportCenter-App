using Microsoft.AspNetCore.Mvc.RazorPages;
using ReportCenter.Web.Models;

namespace ReportCenter.Web.Pages;

public class DepartmentModel : PageModel
{
    public string DeptId { get; set; } = "procurement";
    public Department Dept { get; set; } = null!;
    public List<Report> Reports { get; set; } = [];

    public void OnGet(string dept)
    {
        DeptId = dept ?? "procurement";
        Dept = ReportData.Departments.Find(d => d.Id == DeptId) ?? ReportData.Departments[0];
        Reports = ReportData.Reports.GetValueOrDefault(DeptId) ?? ReportData.Reports["procurement"];
    }
}
