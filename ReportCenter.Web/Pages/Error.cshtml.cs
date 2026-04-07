using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ReportCenter.Web.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    public string? RequestId { get; set; }
    public string? OriginalPath { get; set; }
    public string? ExceptionType { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public void OnGet()
    {
        RequestId = HttpContext.TraceIdentifier ?? Activity.Current?.Id;

        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionFeature?.Error == null)
        {
            return;
        }

        OriginalPath = exceptionFeature.Path ?? HttpContext.Request.Path.Value;
        ExceptionType = exceptionFeature.Error.GetType().Name;
    }
}
