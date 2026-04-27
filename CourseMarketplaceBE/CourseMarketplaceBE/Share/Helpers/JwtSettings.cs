namespace CourseMarketplaceBE.Share.Helpers;

public class JwtSettings
{
    public string Key { get; set; } = "ReplaceThisWithASecretKeyForDevelopmentOnly";
    public string Issuer { get; set; } = "CourseMarketplaceBE";
    public string Audience { get; set; } = "CourseMarketplaceBEClients";
    /// <summary>Thời hạn access token (phút). Đã tăng lên 24h (1440) để SignalR không bị ngắt quãng.</summary>
    public int AccessTokenExpirationMinutes { get; set; } = 1440;
    /// <summary>Thời hạn refresh token (ngày). Production: 7 ngày.</summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
    