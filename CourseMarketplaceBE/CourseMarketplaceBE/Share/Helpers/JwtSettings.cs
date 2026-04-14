namespace CourseMarketplaceBE.Share.Helpers;

public class JwtSettings
{
    public string Key { get; set; } = "ReplaceThisWithASecretKeyForDevelopmentOnly";
    public string Issuer { get; set; } = "CourseMarketplaceBE";
    public string Audience { get; set; } = "CourseMarketplaceBEClients";
    public int AccessTokenExpirationMinutes { get; set; } = 60;
}
    