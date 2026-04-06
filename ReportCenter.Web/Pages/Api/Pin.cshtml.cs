using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReportCenter.Web.Services;

namespace ReportCenter.Web.Pages.Api;

public class PinModel : PageModel
{
    private readonly IReportService _svc;

    public PinModel(IReportService svc) => _svc = svc;

    public IActionResult OnPostToggle(int reportId)
    {
        _svc.TogglePin(reportId);
        var pins = _svc.GetUserPins();
        var isPinned = pins.Any(p => p.ReportID == reportId);
        return new JsonResult(new { success = true, isPinned, count = pins.Count });
    }
}
