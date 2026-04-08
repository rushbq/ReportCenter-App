namespace ReportCenter.Web.Repositories;

using ReportCenter.Web.Models;

/// <summary>
/// PKSYS 資料庫存取介面 (DAL) — User_Dept / User_Profile
/// </summary>
public interface IPksysRepository
{
    /// <summary>從 User_Dept 取得部門清單 (依 Area 過濾)</summary>
    List<CatalogDept> GetCatalogDepartments(string? area = null);

    /// <summary>搜尋使用者 (模糊比對 Account_Name / Display_Name)</summary>
    List<UserProfileItem> SearchUsers(string? keyword = null, string? deptId = null);

    /// <summary>取得單一使用者</summary>
    UserProfileItem? GetUser(string accountName);

    /// <summary>取得某部門所有使用者</summary>
    List<UserProfileItem> GetUsersByDepartment(string deptId);
}
