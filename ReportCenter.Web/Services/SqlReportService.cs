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
    private const string AuthorizedIdsCacheKey = "__AuthorizedReportIds";
    private const string DepartmentsCacheKey = "__Departments";
    private const string ReportsCacheKeyPrefix = "__Reports_";

    private readonly MockReportService _mock = new();
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICatalogRepository _repo;
    private readonly IPksysRepository _pksysRepo;
    private readonly IPermissionRepository _permRepo;
    private readonly DepartmentDisplaySettings _deptDisplay;
    private readonly ReportBaseUrlSettings _baseUrls;
    private readonly ILogger<SqlReportService> _logger;
    private readonly HashSet<string> _adminUsers;

    public SqlReportService(
        IHttpContextAccessor httpContextAccessor,
        ICatalogRepository repo,
        IPksysRepository pksysRepo,
        IPermissionRepository permRepo,
        IOptions<DepartmentDisplaySettings> deptDisplay,
        IOptions<ReportBaseUrlSettings> baseUrls,
        ILogger<SqlReportService> logger,
        IConfiguration config)
    {
        _httpContextAccessor = httpContextAccessor;
        _repo = repo;
        _pksysRepo = pksysRepo;
        _permRepo = permRepo;
        _deptDisplay = deptDisplay.Value;
        _baseUrls = baseUrls.Value;
        _logger = logger;
        _adminUsers = config.GetSection("AdminUsers")
            .Get<string[]>()?
            .ToHashSet(StringComparer.OrdinalIgnoreCase)
            ?? [];
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

        // 未通過驗證時 ResolvedUser 為空使用者 (Id = "")：不具管理員身分、也沒有任何報表權限。
        // 不可退回固定的 Mock 使用者，否則匿名請求會繼承該使用者 (可能是管理員) 的權限。
        var resolvedUser = GetCurrentUserDebugInfo().ResolvedUser;

        if (httpContext != null)
            httpContext.Items[CurrentUserCacheKey] = resolvedUser;

        return resolvedUser;
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
                    NickName = pksysUser?.NickName ?? "",
                    CompanyId = ResolveCompanyId(pksysUser),
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
                pksysLookupError,
                ResolveCompanyId(pksysUser)),
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

    /// <summary>使用者預設公司別：依 PKSYS 所屬部門的 Area (TW / SH) 對應公司，查無時退回台灣寶工</summary>
    private string ResolveCompanyId(UserProfileItem? pksysUser)
        => GetCompanies().Find(c => string.Equals(c.Region, pksysUser?.Area, StringComparison.OrdinalIgnoreCase))?.Id
           ?? "tw";

    private static List<ResolvedUserField> BuildResolvedFields(
        bool isAuthenticated,
        string normalizedAccountName,
        string displayNameClaim,
        string employeeIdClaim,
        string departmentClaim,
        string departmentIdClaim,
        UserProfileItem? pksysUser,
        string pksysLookupError,
        string companyId)
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
            new()
            {
                Label = "UserInfo.CompanyId",
                Value = companyId,
                Source = !string.IsNullOrWhiteSpace(pksysUser?.Area)
                    ? $"PKSYS: User_Dept.Area = {pksysUser.Area}{pksysSourceSuffix}"
                    : "未取得 Area，預設台灣寶工 (tw)"
            },
        ];
    }

    public List<Company> GetCompanies() => _mock.GetCompanies();

    /// <inheritdoc />
    /// <remarks>companyId cookie 只在使用者主動切換公司時才寫入，沒有 cookie 時必須退回使用者預設公司。</remarks>
    public string GetCurrentCompanyId()
    {
        var cookieCompanyId = _httpContextAccessor.HttpContext?.Request.Cookies["companyId"];
        return !string.IsNullOrEmpty(cookieCompanyId) && GetCompanies().Any(c => c.Id == cookieCompanyId)
            ? cookieCompanyId
            : GetCurrentUser().CompanyId;
    }

    /// <inheritdoc />
    public string BuildReportUrl(Report report)
    {
        var url = (report.ReportTool == "SSRS" ? _baseUrls.SSRS : _baseUrls.SmartQuery) + report.ReportCode;
        if (!report.UseCompanyParam) return url;

        var sep = url.Contains('?') ? '&' : '?';
        return $"{url}{sep}dbs={Uri.EscapeDataString(GetCurrentCompanyId())}";
    }

    // ─── 部門（真實資料） ───

    /// <remarks>
    /// 側欄、搜尋索引、頁面本身在同一請求會重複呼叫，故以 HttpContext.Items 做每請求快取；
    /// 公司別與權限在單一請求內不會改變，快取安全。
    /// </remarks>
    public List<Department> GetDepartments() => GetOrAddRequestCache(DepartmentsCacheKey, () =>
    {
        var region = GetCurrentRegion().ToUpper();
        if (!_deptDisplay.Regions.TryGetValue(region, out var config))
            return [];

        var allDepts = _pksysRepo.GetCatalogDepartments(region);
        var result = new List<Department>();

        foreach (var entry in config.Depts)
        {
            var dept = allDepts.Find(d => d.DeptID == entry.DeptID);
            if (dept == null) continue;

            // GetReports 已套用權限過濾 (管理員不過濾)，數量與分類皆以使用者看得到的報表計算
            var permittedReports = GetReports(entry.DeptID);
            var categories = permittedReports
                .Where(r => !string.IsNullOrEmpty(r.Cat))
                .Select(r => r.Cat)
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
    });

    public Department? GetDepartment(string deptId)
        => GetDepartments().Find(d => d.Id == deptId);

    // ─── 報表（真實資料） ───

    public List<Report> GetReports(string deptId) => GetOrAddRequestCache(ReportsCacheKeyPrefix + deptId, () =>
    {
        // ReportDepartment.DeptID 為 INT；非數字代碼不可能有報表，記錄警告後回傳空清單
        if (!int.TryParse(deptId, out var deptIdInt))
        {
            _logger.LogWarning("部門代碼非數字，無法對應報表。DeptID={DeptID}", deptId);
            return [];
        }

        var items = _repo.GetActiveReportsByDepartment(deptIdInt);

        // 依使用者權限過濾 (預設無權限策略)；管理員 (null) 繞過此過濾
        var authorizedSet = GetAuthorizedReportIdSet();
        if (authorizedSet != null)
            items = items.Where(i => authorizedSet.Contains(i.ReportID)).ToList();

        return items.Select(ToReport).ToList();
    });

    public Report? GetReport(string deptId, string reportName)
        => GetReports(deptId).Find(r => r.Name == reportName);

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

        // 依權限過濾：僅回傳使用者有權限的收藏；管理員 (null) 不過濾
        var authorizedSet = GetAuthorizedReportIdSet();
        return authorizedSet == null
            ? favorites
            : favorites.Where(id => authorizedSet.Contains(id)).ToList();
    }

    public bool ToggleFavorite(int reportId)
    {
        var userId = GetCurrentUser().Id;
        if (string.IsNullOrEmpty(userId)) return false;

        var favorites = _repo.GetUserFavorites(userId);
        if (favorites.Contains(reportId))
            _repo.RemoveFavorite(userId, reportId);   // 取消一律允許 (即使已失去權限)
        else if (CanAccessReport(reportId))
            _repo.AddFavorite(userId, reportId);
        else
            return false;

        return true;
    }

    // ─── 釘選 ───

    public List<Report> GetUserPins()
    {
        var userId = GetCurrentUser().Id;
        var items = _repo.GetUserPins(userId);

        // 依權限過濾釘選列表；管理員 (null) 不過濾
        var authorizedSet = GetAuthorizedReportIdSet();
        if (authorizedSet != null)
            items = items.Where(i => authorizedSet.Contains(i.ReportID)).ToList();

        return items.Select(ToReport).ToList();
    }

    public bool TogglePin(int reportId)
    {
        var userId = GetCurrentUser().Id;
        if (string.IsNullOrEmpty(userId)) return false;

        var pins = _repo.GetUserPins(userId);
        if (pins.Any(p => p.ReportID == reportId))
            _repo.RemovePin(userId, reportId);        // 取消一律允許 (即使已失去權限)
        else if (CanAccessReport(reportId))
            _repo.AddPin(userId, reportId);
        else
            return false;

        return true;
    }

    // ─── 輔助方法 ───

    /// <summary>
    /// 取得目前使用者已授權的 ReportID 集合，每請求快取一次以避免重複 DB 查詢。
    /// 回傳 <c>null</c> 代表管理員 (不過濾，可看見所有啟用中的報表)。
    /// </summary>
    private HashSet<int>? GetAuthorizedReportIdSet()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Items.TryGetValue(AuthorizedIdsCacheKey, out var cached) == true)
            return (HashSet<int>?)cached;

        var userId = GetCurrentUser().Id;
        HashSet<int>? set = _adminUsers.Contains(userId)
            ? null
            : new HashSet<int>(_permRepo.GetAuthorizedReportIds(userId));

        if (httpContext != null)
            httpContext.Items[AuthorizedIdsCacheKey] = set;

        return set;
    }

    /// <summary>
    /// 使用者是否可存取指定報表 (新增收藏/釘選前檢查)：報表須存在且啟用；
    /// 一般使用者另須有授權，管理員則不需授權。
    /// </summary>
    private bool CanAccessReport(int reportId)
    {
        if (_repo.GetCatalogItem(reportId) is not { IsActive: true }) return false;

        var authorizedSet = GetAuthorizedReportIdSet();
        return authorizedSet == null || authorizedSet.Contains(reportId);
    }

    /// <summary>每請求快取：同一請求內重複呼叫只查一次 DB (無 HttpContext 時直接計算)</summary>
    private T GetOrAddRequestCache<T>(string key, Func<T> factory)
    {
        var items = _httpContextAccessor.HttpContext?.Items;
        if (items != null && items.TryGetValue(key, out var cached) && cached is T value)
            return value;

        var result = factory();
        if (items != null)
            items[key] = result;
        return result;
    }

    private string GetCurrentRegion()
    {
        var companyId = GetCurrentCompanyId();
        return GetCompanies().Find(c => c.Id == companyId)?.Region ?? "TW";
    }

    private static Report ToReport(ReportCatalogItem item) => new()
    {
        ReportID = item.ReportID,
        Name = item.ReportName,
        Desc = item.Remarks,
        Cat = item.CategoryName,
        // yyyy/MM/dd 零填格式：前端可直接字串排序，且跨年份篩選不會誤判
        // InvariantCulture 確保 "/" 不被伺服器文化替換為其他分隔符
        Updated = (item.ModifyDate ?? item.CreateDate).ToString("yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture),
        ReportTool = item.ReportTool,
        ReportCode = item.ReportCode,
        UseCompanyParam = item.UseCompanyParam,
    };
}
