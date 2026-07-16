using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReportCenter.Web.Services;

namespace ReportCenter.Web.Pages.Api;

/// <summary>
/// 報表使用記錄 API — 前端 sendBeacon 埋點呼叫，
/// 防重複 (30 秒) 由 UsageService 控制。
/// </summary>
public class UsageModel : PageModel
{
    private readonly IUsageService _svc;

    public UsageModel(IUsageService svc) => _svc = svc;

    public IActionResult OnPostTrack(int reportId, string? source)
    {
        var recorded = _svc.TrackClick(reportId, source);
        return new JsonResult(new { success = true, recorded });
    }
}
