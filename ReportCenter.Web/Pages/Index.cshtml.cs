using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using ReportCenter.Web.Models;
using ReportCenter.Web.Models.Settings;
using ReportCenter.Web.Services;

namespace ReportCenter.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IReportService _svc;
    private readonly ReportBaseUrlSettings _baseUrls;

    public IndexModel(IReportService svc, IOptions<ReportBaseUrlSettings> baseUrls)
    {
        _svc = svc;
        _baseUrls = baseUrls.Value;
    }

    public List<object> AllReports { get; set; } = [];
    public List<Report> PinnedReports { get; set; } = [];
    public List<int> PinnedIds { get; set; } = [];
    public List<Department> Departments { get; set; } = [];

    public string SmartQueryBaseUrl => _baseUrls.SmartQuery;
    public string SsrsBaseUrl => _baseUrls.SSRS;

    public void OnGet()
    {
        Departments = _svc.GetDepartments();

        // 組裝所有報表清單供釘選 Modal 使用
        foreach (var dept in Departments)
        {
            foreach (var report in _svc.GetReports(dept.Id))
            {
                AllReports.Add(new {
                    reportId = report.ReportID,
                    name = report.Name,
                    dept = dept.Label,
                    deptId = dept.Id,
                    cat = report.Cat,
                    reportTool = report.ReportTool,
                    reportCode = report.ReportCode
                });
            }
        }

        // 釘選資料
        PinnedReports = _svc.GetUserPins();
        PinnedIds = PinnedReports.Select(p => p.ReportID).ToList();
    }
}
