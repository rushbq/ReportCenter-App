using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReportCenter.Web.Services;

namespace ReportCenter.Web.Pages.Api;

public class FavoriteModel : PageModel
{
    private readonly IReportService _svc;

    public FavoriteModel(IReportService svc) => _svc = svc;

    public IActionResult OnPostToggle(int reportId)
    {
        _svc.ToggleFavorite(reportId);
        var favorites = _svc.GetUserFavorites();
        var isFav = favorites.Contains(reportId);
        return new JsonResult(new { success = true, isFavorite = isFav, count = favorites.Count });
    }
}
