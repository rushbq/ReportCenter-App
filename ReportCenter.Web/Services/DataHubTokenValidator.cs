namespace ReportCenter.Web.Services;

using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using ReportCenter.Web.Models.Settings;

/// <summary>
/// 以 DataHubApi 的公鑰驗證 JWT。Token 內容不寫入日誌。
/// </summary>
public partial class DataHubTokenValidator : IDataHubTokenValidator
{
    private readonly JsonWebTokenHandler _handler = new();
    private readonly DataHubAuthSettings _settings;
    private readonly SecurityKey? _key;
    private readonly ILogger<DataHubTokenValidator> _logger;

    public DataHubTokenValidator(IOptions<DataHubAuthSettings> settings, ILogger<DataHubTokenValidator> logger)
    {
        _settings = settings.Value;
        _key = _settings.GetValidationKey();
        _logger = logger;
    }

    // 與 DataHubApi AuthService.NormalizeUsername 相同的帳號字元規則
    [GeneratedRegex(@"^[A-Za-z0-9._-]{1,64}$")]
    private static partial Regex AccountPattern();

    /// <inheritdoc />
    public async Task<string?> ValidateAsync(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        if (_key == null)
        {
            _logger.LogError("未設定 DataHubAuth:PublicKeyPem 或公鑰格式錯誤，無法驗證登入 Token");
            return null;
        }

        var result = await _handler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidIssuer = _settings.Issuer,
            ValidAudience = _settings.Audience,
            IssuerSigningKey = _key,
            ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
            ClockSkew = TimeSpan.FromMinutes(1),
        });

        if (!result.IsValid)
        {
            _logger.LogWarning("登入 Token 驗證失敗：{Reason}", result.Exception?.GetType().Name ?? "未知原因");
            return null;
        }

        var name = result.Claims.TryGetValue(JwtRegisteredClaimNames.Name, out var value) ? value as string : null;
        var account = NormalizeAccount(name);
        if (account == null)
            _logger.LogWarning("登入 Token 的 name claim 不是有效的工號");

        return account;
    }

    /// <summary>將 PROKITS\10308、10308@prokits.com.tw 統一為 10308；格式不合法時回傳 null</summary>
    private static string? NormalizeAccount(string? input)
    {
        var name = input?.Trim() ?? "";
        var slash = name.LastIndexOf('\\');
        if (slash >= 0) name = name[(slash + 1)..];
        var at = name.IndexOf('@');
        if (at >= 0) name = name[..at];
        return AccountPattern().IsMatch(name) ? name : null;
    }
}
