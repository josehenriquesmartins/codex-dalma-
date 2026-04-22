namespace Dalba.Financeiro.Infrastructure.Configuration;

public class JwtSettings
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "Dalba";
    public string Audience { get; set; } = "Dalba";
    public string Key { get; set; } = "DALBA_SUPER_SECRET_KEY_2026_CHANGE_ME";
    public int ExpirationHours { get; set; } = 8;
}
