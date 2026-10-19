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
        if (!_svc.TogglePin(reportId))
            return new JsonResult(new { success = false, message = "找不到報表或無存取權限" })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };

        var pins = _svc.GetUserPins();
        var isPinned = pins.Any(p => p.ReportID == reportId);
        return new JsonResult(new { success = true, isPinned, count = pins.Count });
    }
}
