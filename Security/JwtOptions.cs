namespace GuardianPet.Security;

public sealed class JwtOptions
{
    public const string DevelopmentKey = "GuardianPet-Development-Only-Replace-This-Key-2026!";
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
    public long[] AdminUserIds { get; set; } = Array.Empty<long>();
}
