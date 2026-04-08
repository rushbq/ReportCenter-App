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
    private const string CurrentUserCacheKey = "__CurrentUser";
    private const string CurrentUserDebugCacheKey = "__CurrentUserDebug";

    private readonly MockReportService _mock = new();
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICatalogRepository _repo;
    private readonly IPksysRepository _pksysRepo;
    private readonly IPermissionRepository _permRepo;
    private readonly DepartmentDisplaySettings _deptDisplay;
    private readonly ReportBaseUrlSettings _baseUrls;
    private readonly ILogger<SqlReportService> _logger;

    public SqlReportService(
        IHttpContextAccessor httpContextAccessor,
        ICatalogRepository repo,
        IPksysRepository pksysRepo,
        IPermissionRepository permRepo,
        IOptions<DepartmentDisplaySettings> deptDisplay,
        IOptions<ReportBaseUrlSettings> baseUrls,
        ILogger<SqlReportService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _repo = repo;
        _pksysRepo = pksysRepo;
        _permRepo = permRepo;
        _deptDisplay = deptDisplay.Value;
        _baseUrls = baseUrls.Value;
        _logger = logger;
    }

    // ─── 使用者與公司 ───

    public UserInfo GetCurrentUser()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Items.TryGetValue(CurrentUserCacheKey, out var cachedUser) == true &&
            cachedUser is UserInfo userInfo)
        {
            return userInfo;
        }

        var debugInfo = GetCurrentUserDebugInfo();
        if (!debugInfo.IsAuthenticated)
            return _mock.GetCurrentUser();

        if (httpContext != null)
            httpContext.Items[CurrentUserCacheKey] = debugInfo.ResolvedUser;

        return debugInfo.ResolvedUser;
    }

    /// <summary>
    /// 取得目前登入者的 Windows 驗證診斷資訊。
    /// 此方法與 GetCurrentUser() 共用相同解析規則，供 IIS 正式環境除錯頁使用。
    /// </summary>
    public WindowsAuthDebugInfo GetCurrentUserDebugInfo()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Items.TryGetValue(CurrentUserDebugCacheKey, out var cachedDebugInfo) == true &&
            cachedDebugInfo is WindowsAuthDebugInfo debugInfo)
        {
            return debugInfo;
        }

        var resolved = BuildCurrentUserDebugInfo(httpContext?.User);
        if (httpContext != null)
            httpContext.Items[CurrentUserDebugCacheKey] = resolved;

        return resolved;
    }

    private WindowsAuthDebugInfo BuildCurrentUserDebugInfo(ClaimsPrincipal? principal)
    {
        var traceId = _httpContextAccessor.HttpContext?.TraceIdentifier ?? "";
        var identity = principal?.Identity as ClaimsIdentity;
        var accountName = principal?.Identity?.Name ?? "";
        var normalizedAccountName = NormalizeAccountName(accountName);
        var displayNameClaim = principal?.FindFirst("DisplayName")?.Value ?? "";
        var employeeIdClaim = NormalizeAccountName(principal?.FindFirst("EmployeeId")?.Value ?? "");
        var departmentClaim = principal?.FindFirst("Department")?.Value ?? "";
        var departmentIdClaim = principal?.FindFirst("DepartmentId")?.Value ?? "";
        var isAuthenticated = principal?.Identity?.IsAuthenticated == true;
        var lookupAccount = !string.IsNullOrWhiteSpace(employeeIdClaim)
            ? employeeIdClaim
            : normalizedAccountName;

        UserProfileItem? pksysUser = null;
        var pksysLookupError = "";

        if (isAuthenticated && !string.IsNullOrWhiteSpace(lookupAccount))
        {
            try
            {
                pksysUser = _pksysRepo.GetUser(lookupAccount);
            }
            catch (Exception ex)
            {
                pksysLookupError = ex.Message;
                _logger.LogError(ex,
                    "取得目前登入者 PKSYS 資料失敗。LookupAccount={LookupAccount} TraceId={TraceId}",
                    lookupAccount,
                    traceId);
            }
        }

        return new WindowsAuthDebugInfo
        {
            IsAuthenticated = isAuthenticated,
            AuthenticationType = principal?.Identity?.AuthenticationType ?? "",
            IdentityName = accountName,
            NormalizedAccountName = normalizedAccountName,
            NameClaimType = identity?.NameClaimType ?? "",
            RoleClaimType = identity?.RoleClaimType ?? "",
            PksysLookupAccount = lookupAccount,
            PksysUserFound = pksysUser != null,
            PksysLookupError = pksysLookupError,
            ResolvedUser = isAuthenticated
                ? new UserInfo
                {
                    Id = ResolveEmployeeId(employeeIdClaim, normalizedAccountName, pksysUser),
                    Name = ResolveDisplayName(displayNameClaim, normalizedAccountName, pksysUser),
                    DeptId = ResolveDepartmentId(departmentIdClaim, pksysUser),
                    DeptName = ResolveDepartmentName(departmentClaim, pksysUser),
                }
                : new UserInfo(),
            ResolvedFields = BuildResolvedFields(
                isAuthenticated,
                normalizedAccountName,
                displayNameClaim,
                employeeIdClaim,
                departmentClaim,
                departmentIdClaim,
                pksysUser,
                pksysLookupError),
            Claims = principal?.Claims
                .Select(c => new AuthClaimInfo
                {
                    Type = c.Type,
                    Value = c.Value,
                    ValueType = c.ValueType,
                    Issuer = c.Issuer,
                    OriginalIssuer = c.OriginalIssuer,
                })
                .OrderBy(c => c.Type, StringComparer.Ordinal)
                .ThenBy(c => c.Value, StringComparer.Ordinal)
                .ToList() ?? [],
        };
    }

    private static string ExtractUserName(string accountName)
    {
        var idx = accountName.IndexOf('\\');
        return idx >= 0 ? accountName[(idx + 1)..] : accountName;
    }

    private static string NormalizeAccountName(string accountName)
        => string.IsNullOrWhiteSpace(accountName)
            ? ""
            : ExtractUserName(accountName.Trim());

    private static string ResolveDisplayName(string displayNameClaim, string normalizedAccountName, UserProfileItem? pksysUser)
        => !string.IsNullOrWhiteSpace(displayNameClaim)
            ? displayNameClaim
            : !string.IsNullOrWhiteSpace(pksysUser?.DisplayName)
                ? pksysUser.DisplayName
                : normalizedAccountName;

    private static string ResolveEmployeeId(string employeeIdClaim, string normalizedAccountName, UserProfileItem? pksysUser)
        => !string.IsNullOrWhiteSpace(employeeIdClaim)
            ? employeeIdClaim
            : !string.IsNullOrWhiteSpace(pksysUser?.AccountName)
                ? pksysUser.AccountName
                : normalizedAccountName;

    private static string ResolveDepartmentId(string departmentIdClaim, UserProfileItem? pksysUser)
        => !string.IsNullOrWhiteSpace(departmentIdClaim)
            ? departmentIdClaim
            : pksysUser?.DeptID ?? "";

    private static string ResolveDepartmentName(string departmentClaim, UserProfileItem? pksysUser)
        => !string.IsNullOrWhiteSpace(departmentClaim)
            ? departmentClaim
            : pksysUser?.DeptName ?? "";

    private static List<ResolvedUserField> BuildResolvedFields(
        bool isAuthenticated,
        string normalizedAccountName,
        string displayNameClaim,
        string employeeIdClaim,
        string departmentClaim,
        string departmentIdClaim,
        UserProfileItem? pksysUser,
        string pksysLookupError)
    {
        if (!isAuthenticated)
        {
            return
            [
                new() { Label = "UserInfo.Id", Value = "", Source = "未通過 Windows 驗證" },
                new() { Label = "UserInfo.Name", Value = "", Source = "未通過 Windows 驗證" },
                new() { Label = "UserInfo.DeptId", Value = "", Source = "未通過 Windows 驗證" },
                new() { Label = "UserInfo.DeptName", Value = "", Source = "未通過 Windows 驗證" },
            ];
        }

        var pksysSourceSuffix = !string.IsNullOrWhiteSpace(pksysLookupError)
            ? "（PKSYS 查詢失敗）"
            : pksysUser != null
                ? ""
                : "（PKSYS 無對應資料）";

        return
        [
            new()
            {
                Label = "PKSYS.LookupAccount",
                Value = !string.IsNullOrWhiteSpace(employeeIdClaim) ? employeeIdClaim : normalizedAccountName,
                Source = !string.IsNullOrWhiteSpace(employeeIdClaim)
                    ? "優先使用 Claim: EmployeeId"
                    : "Fallback: Identity.Name 去除網域"
            },
            new()
            {
                Label = "UserInfo.Id",
                Value = ResolveEmployeeId(employeeIdClaim, normalizedAccountName, pksysUser),
                Source = !string.IsNullOrWhiteSpace(employeeIdClaim)
                    ? "Claim: EmployeeId"
                    : !string.IsNullOrWhiteSpace(pksysUser?.AccountName)
                        ? $"PKSYS: User_Profile.Account_Name{pksysSourceSuffix}"
                        : "Fallback: Identity.Name 去除網域"
            },
            new()
            {
                Label = "UserInfo.Name",
                Value = ResolveDisplayName(displayNameClaim, normalizedAccountName, pksysUser),
                Source = !string.IsNullOrWhiteSpace(displayNameClaim)
                    ? "Claim: DisplayName"
                    : !string.IsNullOrWhiteSpace(pksysUser?.DisplayName)
                        ? $"PKSYS: User_Profile.Display_Name{pksysSourceSuffix}"
                        : "Fallback: Identity.Name 去除網域"
            },
            new()
            {
                Label = "UserInfo.DeptId",
                Value = ResolveDepartmentId(departmentIdClaim, pksysUser),
                Source = !string.IsNullOrWhiteSpace(departmentIdClaim)
                    ? "Claim: DepartmentId"
                    : !string.IsNullOrWhiteSpace(pksysUser?.DeptID)
                        ? $"PKSYS: User_Profile.DeptID{pksysSourceSuffix}"
                        : "未取得"
            },
            new()
            {
                Label = "UserInfo.DeptName",
                Value = ResolveDepartmentName(departmentClaim, pksysUser),
                Source = !string.IsNullOrWhiteSpace(departmentClaim)
                    ? "Claim: Department"
                    : !string.IsNullOrWhiteSpace(pksysUser?.DeptName)
                        ? $"PKSYS: User_Dept.DeptName{pksysSourceSuffix}"
                        : "未取得"
            },
        ];
    }

    public List<Company> GetCompanies() => _mock.GetCompanies();

    // ─── 部門（真實資料） ───

    public List<Department> GetDepartments()
    {
        var region = GetCurrentRegion().ToUpper();
        if (!_deptDisplay.Regions.TryGetValue(region, out var config))
            return [];

        var allDepts = _pksysRepo.GetCatalogDepartments(region);

        // 取得使用者已授權的 ReportID，用於計算權限過濾後的數量
        var userId = GetCurrentUser().Id;
        var authorizedIds = _permRepo.GetAuthorizedReportIds(userId);
        var authorizedSet = new HashSet<int>(authorizedIds);

        var result = new List<Department>();

        foreach (var entry in config.Depts)
        {
            var dept = allDepts.Find(d => d.DeptID == entry.DeptID);
            if (dept == null) continue;

            var deptIdInt = int.TryParse(entry.DeptID, out var id) ? id : 0;

            // 依權限過濾：僅計算使用者有權限的報表
            var deptReports = _repo.GetActiveReportsByDepartment(deptIdInt);
            var permittedReports = deptReports.Where(r => authorizedSet.Contains(r.ReportID)).ToList();
            var categories = permittedReports
                .Where(r => !string.IsNullOrEmpty(r.CategoryName))
                .Select(r => r.CategoryName)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            result.Add(new Department
            {
                Id = entry.DeptID,
                Label = dept.DeptName,
                Icon = entry.Icon,
                Count = permittedReports.Count,
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

        // 依使用者權限過濾 (預設無權限策略)
        var userId = GetCurrentUser().Id;
        var authorizedIds = _permRepo.GetAuthorizedReportIds(userId);
        items = items.Where(i => authorizedIds.Contains(i.ReportID)).ToList();

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
        var favorites = _repo.GetUserFavorites(userId);

        // 依權限過濾：僅回傳使用者有權限的收藏
        var authorizedIds = _permRepo.GetAuthorizedReportIds(userId);
        var authorizedSet = new HashSet<int>(authorizedIds);
        return favorites.Where(id => authorizedSet.Contains(id)).ToList();
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

        // 依權限過濾釘選列表
        var authorizedIds = _permRepo.GetAuthorizedReportIds(userId);
        var authorizedSet = new HashSet<int>(authorizedIds);
        items = items.Where(i => authorizedSet.Contains(i.ReportID)).ToList();

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
        UseCompanyParam = item.UseCompanyParam,
    };
}
