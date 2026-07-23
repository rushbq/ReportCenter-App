using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReportCenter.Web.Services;

namespace ReportCenter.Web.Pages.Api;

/// <summary>
/// 首頁年度目標戰情資料 API — 供前端切換模式/區間時 fetch 局部更新 (卡片 + 趨勢圖)。
/// </summary>
public class HomeDashboardModel : PageModel
{
    private readonly IHomeDashboardService _svc;

    public HomeDashboardModel(IHomeDashboardService svc) => _svc = svc;

    public IActionResult OnGetData(string? mode, string? cumType)
    {
        var d = _svc.GetYtdTarget(mode, cumType);
        return new JsonResult(new
        {
            mode = d.Mode,
            cumType = d.CumulativeType,
            reportYear = d.ReportYear,
            endMonth = d.EndMonth,
            blocks = d.Blocks,
            trend = d.Trend,
        });
    }
}
