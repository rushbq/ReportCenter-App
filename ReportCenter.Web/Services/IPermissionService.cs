namespace ReportCenter.Web.Services;

using ReportCenter.Web.Models;

/// <summary>
/// 權限管理商業邏輯介面 (BLL)。
/// 職責：報表權限授權/撤銷、使用者權限查詢、權限管理 UI 所需的樹狀資料。
/// </summary>
public interface IPermissionService
{
    /// <summary>批次授權 (冪等，回傳實際新增數)</summary>
    int GrantBatch(List<string> employeeIds, List<int> reportIds, string grantedBy);

    /// <summary>取得指定使用者的所有權限</summary>
    List<UserReportPermission> GetUserPermissions(string employeeId);

    /// <summary>撤銷單一權限</summary>
    bool RevokePermission(string employeeId, int reportId);

    /// <summary>檢查使用者是否有該報表權限</summary>
    bool HasPermission(string employeeId, int reportId);

    /// <summary>取得使用者所有已授權的 ReportID</summary>
    List<int> GetAuthorizedReportIds(string employeeId);

    /// <summary>取得部門→使用者樹 (權限管理 UI 用)</summary>
    List<DeptWithUsers> GetDeptUserTree();

    /// <summary>取得分類→報表樹 (權限管理 UI 用)</summary>
    List<CategoryWithReports> GetReportTree();

    /// <summary>搜尋使用者 (模糊比對)</summary>
    List<UserProfileItem> SearchUsers(string? keyword);
}
