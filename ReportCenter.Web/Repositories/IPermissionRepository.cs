namespace ReportCenter.Web.Repositories;

using ReportCenter.Web.Models;

/// <summary>
/// 使用者報表權限資料存取介面 (DAL) — ReportCenter 資料庫
/// </summary>
public interface IPermissionRepository
{
    /// <summary>取得指定使用者的所有權限 (含報表名稱/分類)</summary>
    List<UserReportPermission> GetPermissionsByUser(string employeeId);

    /// <summary>取得指定報表的所有授權使用者</summary>
    List<UserReportPermission> GetPermissionsByReport(int reportId);

    /// <summary>批次授權 (冪等，回傳實際新增數)</summary>
    int GrantPermissions(List<string> employeeIds, List<int> reportIds, string grantedBy);

    /// <summary>撤銷單一權限</summary>
    bool RevokePermission(string employeeId, int reportId);

    /// <summary>檢查使用者是否有該報表權限</summary>
    bool HasPermission(string employeeId, int reportId);

    /// <summary>取得使用者所有已授權的 ReportID</summary>
    List<int> GetAuthorizedReportIds(string employeeId);
}
