namespace ReportCenter.Web.Services;

using System.Security.Claims;
using Microsoft.Extensions.Options;
using ReportCenter.Web.Models;
using ReportCenter.Web.Models.Settings;
using ReportCenter.Web.Repositories;

/// <summary>
/// SQL Server 實作 — 部門/報表/收藏/釘選使用真實資料
/// 儀表板 KPI / 圖表仍沿用 Mock 資料
/// </summary>
public class SqlReportService : IReportService
{
    private readonly MockReportService _mock = new();
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICatalogRepository _repo;
    private readonly DepartmentDisplaySettings _deptDisplay;
    private readonly ReportBaseUrlSettings _baseUrls;

    public SqlReportService(
        IHttpContextAccessor httpContextAccessor,
        ICatalogRepository repo,
        IOptions<DepartmentDisplaySettings> deptDisplay,
        IOptions<ReportBaseUrlSettings> baseUrls)
    {
        _httpContextAccessor = httpContextAccessor;
        _repo = repo;
        _deptDisplay = deptDisplay.Value;
        _baseUrls = baseUrls.Value;
    }

    // ─── 使用者與公司 ───

    public UserInfo GetCurrentUser()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
            return _mock.GetCurrentUser();

        var accountName = principal.Identity.Name ?? "";
        var displayName = principal.FindFirst("DisplayName")?.Value ?? "";
        var empId = principal.FindFirst("EmployeeId")?.Value ?? "";
        var deptName = principal.FindFirst("Department")?.Value ?? "";
        var deptId = principal.FindFirst("DepartmentId")?.Value ?? "";

        if (string.IsNullOrEmpty(displayName))
            displayName = ExtractUserName(accountName);
        if (string.IsNullOrEmpty(empId))
            empId = accountName;

        return new UserInfo
        {
            Id = empId,
            Name = displayName,
            DeptId = deptId,
            DeptName = deptName,
        };
    }

    private static string ExtractUserName(string accountName)
    {
        var idx = accountName.IndexOf('\\');
        return idx >= 0 ? accountName[(idx + 1)..] : accountName;
    }

    public List<Company> GetCompanies() => _mock.GetCompanies();

    // ─── 部門（真實資料） ───

    public List<Department> GetDepartments()
    {
        var region = GetCurrentRegion().ToUpper();
        if (!_deptDisplay.Regions.TryGetValue(region, out var config))
            return [];

        var allDepts = _repo.GetCatalogDepartments(region);
        var result = new List<Department>();

        foreach (var entry in config.Depts)
        {
            var dept = allDepts.Find(d => d.DeptID == entry.DeptID);
            if (dept == null) continue;

            var deptIdInt = int.TryParse(entry.DeptID, out var id) ? id : 0;
            var categories = _repo.GetCategoriesByDepartment(deptIdInt);
            var count = _repo.GetActiveReportCountByDepartment(deptIdInt);

            result.Add(new Department
            {
                Id = entry.DeptID,
                Label = dept.DeptName,
                Icon = entry.Icon,
                Count = count,
                Subs = categories,
            });
        }

        return result;
    }

    public Department? GetDepartment(string deptId)
        => GetDepartments().Find(d => d.Id == deptId);

    // ─── 報表（真實資料） ───

    public List<Report> GetReports(string deptId)
    {
        var deptIdInt = int.TryParse(deptId, out var id) ? id : 0;
        var items = _repo.GetActiveReportsByDepartment(deptIdInt);

        return items.Select(ToReport).ToList();
    }

    public Report? GetReport(string deptId, string reportName)
        => GetReports(deptId).Find(r => r.Name == reportName);

    // ─── 快速存取（由釘選功能取代） ───

    public List<QuickAccess> GetQuickAccessItems() => [];

    // ─── 明細資料（維持 Mock） ───

    public List<MaterialRow> GetMaterialRows(string deptId, string reportName, int page = 1, int pageSize = 20)
        => _mock.GetMaterialRows(deptId, reportName, page, pageSize);

    public int GetMaterialRowCount(string deptId, string reportName)
        => _mock.GetMaterialRowCount(deptId, reportName);

    // ─── 儀表板 KPI（維持 Mock） ───

    public DashboardKpi GetDashboardKpi(string companyId)
        => _mock.GetDashboardKpi(companyId);

    // ─── 圖表資料（維持 Mock） ───

    public ChartData GetRevenueChartData(string companyId, string period = "month")
        => _mock.GetRevenueChartData(companyId, period);

    public ChartData GetDeptComparisonData(string companyId)
        => _mock.GetDeptComparisonData(companyId);

    public ChartData GetReportChartData(string deptId, string reportName, string chartType)
        => _mock.GetReportChartData(deptId, reportName, chartType);

    // ─── 收藏 ───

    public List<int> GetUserFavorites()
    {
        var userId = GetCurrentUser().Id;
        return _repo.GetUserFavorites(userId);
    }

    public void ToggleFavorite(int reportId)
    {
        var userId = GetCurrentUser().Id;
        var favorites = _repo.GetUserFavorites(userId);
        if (favorites.Contains(reportId))
            _repo.RemoveFavorite(userId, reportId);
        else
            _repo.AddFavorite(userId, reportId);
    }

    // ─── 釘選 ───

    public List<Report> GetUserPins()
    {
        var userId = GetCurrentUser().Id;
        var items = _repo.GetUserPins(userId);
        return items.Select(ToReport).ToList();
    }

    public void TogglePin(int reportId)
    {
        var userId = GetCurrentUser().Id;
        var pins = _repo.GetUserPins(userId);
        if (pins.Any(p => p.ReportID == reportId))
            _repo.RemovePin(userId, reportId);
        else
            _repo.AddPin(userId, reportId);
    }

    // ─── 輔助方法 ───

    private string GetCurrentRegion()
    {
        var cookieCompanyId = _httpContextAccessor.HttpContext?.Request.Cookies["companyId"];
        var companies = GetCompanies();
        var company = companies.Find(c => c.Id == cookieCompanyId) ?? companies.FirstOrDefault();
        return company?.Region ?? "TW";
    }

    private static Report ToReport(ReportCatalogItem item) => new()
    {
        ReportID = item.ReportID,
        Name = item.ReportName,
        Desc = item.Remarks,
        Cat = item.CategoryName,
        Updated = (item.ModifyDate ?? item.CreateDate).ToString("MM/dd"),
        ReportTool = item.ReportTool,
        ReportCode = item.ReportCode,
    };
}
