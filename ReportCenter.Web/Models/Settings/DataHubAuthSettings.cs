using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace ReportCenter.Web.Models.Settings;

/// <summary>
/// DataHubApi 登入設定 — 對應 appsettings 的 DataHubAuth 區段。
/// 帳密驗證與 Refresh Token 都由 DataHubApi 負責（與 PKPortal 共用），
/// 本站只用公鑰驗證 DataHubApi 發出的 RS256 JWT，再發自己的 Cookie。
/// </summary>
public class DataHubAuthSettings
{
    public const string SectionName = "DataHubAuth";

    /// <summary>DataHubApi 的 API 根網址（含 /api），例如 https://datahubapi.prokits.com.tw/api</summary>
    public string ApiBaseUrl { get; set; } = "";

    /// <summary>JWT 發行者，須與 DataHubApi 的 Jwt:Issuer 相同</summary>
    public string Issuer { get; set; } = "DataHubApi";

    /// <summary>JWT 使用對象，須與 DataHubApi 的 Jwt:Audience 相同</summary>
    public string Audience { get; set; } = "PKPortal";

    /// <summary>DataHubApi 的 RS256 公鑰（PEM，換行寫成 \n）；公鑰不是機密</summary>
    public string PublicKeyPem { get; set; } = "";

    /// <summary>本站登入 Cookie 有效小時數，期間內有使用就會延長</summary>
    public int CookieHours { get; set; } = 8;

    /// <summary>由公鑰建立驗證用金鑰；未設定或格式錯誤時回傳 null</summary>
    public RsaSecurityKey? GetValidationKey()
    {
        if (string.IsNullOrWhiteSpace(PublicKeyPem))
            return null;

        var rsa = RSA.Create();
        try
        {
            rsa.ImportFromPem(PublicKeyPem);
        }
        catch (ArgumentException)
        {
            rsa.Dispose();
            return null;
        }

        return new RsaSecurityKey(rsa);
    }

    /// <summary>非開發環境的啟動檢查：API 網址必須是 https，公鑰必須可解析</summary>
    public bool IsValidForProduction()
        => Uri.TryCreate(ApiBaseUrl, UriKind.Absolute, out var uri)
           && uri.Scheme == Uri.UriSchemeHttps
           && GetValidationKey() != null
           && CookieHours > 0;
}
