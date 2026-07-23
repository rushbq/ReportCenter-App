using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using ReportCenter.Web.Models;
using ReportCenter.Web.Models.Settings;
using ReportCenter.Web.Services;

namespace ReportCenter.Web.Pages;

/// <summary>
/// 首頁營運儀表板。
/// </summary>
public class IndexModel : PageModel
{
    private readonly IReportService _svc;
    private readonly IHomeDashboardService _dashSvc;
    private readonly ReportBaseUrlSettings _baseUrls;

    public IndexModel(IReportService svc, IHomeDashboardService dashSvc, IOptions<ReportBaseUrlSettings> baseUrls)
    {
        _svc = svc;
        _dashSvc = dashSvc;
        _baseUrls = baseUrls.Value;
    }

    public List<object> AllReports { get; set; } = [];
    public List<object> PinnedReportLinks { get; set; } = [];
    public List<Report> PinnedReports { get; set; } = [];
    public List<int> PinnedIds { get; set; } = [];
    public List<Department> Departments { get; set; } = [];

    /// <summary>首頁年度目標戰情初始資料 (預設：銷售 + 即時累計)，切換由前端 fetch 更新。</summary>
    public HomeYtdDashboard Dashboard { get; set; } = new();

    public string SmartQueryBaseUrl => _baseUrls.SmartQuery;
    public string SsrsBaseUrl => _baseUrls.SSRS;

    public void OnGet()
    {
        Dashboard = _dashSvc.GetYtdTarget("S", "R");
        Departments = _svc.GetDepartments();
        var reportsById = new Dictionary<int, (Report Report, string Department)>();

        // 組裝所有報表清單供釘選 Modal 使用
        foreach (var dept in Departments)
        {
            foreach (var report in _svc.GetReports(dept.Id))
            {
                reportsById[report.ReportID] = (report, dept.Label);
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

        foreach (var pinned in PinnedReports)
        {
            if (!reportsById.TryGetValue(pinned.ReportID, out var reportInfo))
                continue;

            PinnedReportLinks.Add(new
            {
                pinned.ReportID,
                pinned.Name,
                pinned.Cat,
                pinned.ReportTool,
                href = BuildReportHref(pinned),
                target = "_blank",
                dept = reportInfo.Department
            });
        }
    }

    private string BuildReportHref(Report report) =>
        report.ReportTool == "SSRS"
            ? _baseUrls.SSRS + report.ReportCode
            : _baseUrls.SmartQuery + report.ReportCode;
}
