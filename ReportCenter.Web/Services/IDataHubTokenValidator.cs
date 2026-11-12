namespace ReportCenter.Web.Services;

/// <summary>
/// 驗證 DataHubApi 發出的 JWT（RS256），取出登入者工號
/// </summary>
public interface IDataHubTokenValidator
{
    /// <summary>
    /// 驗證簽章、發行者、使用對象與有效期限；通過時回傳工號（已去除網域），否則回傳 null
    /// </summary>
    Task<string?> ValidateAsync(string? token);
}
