namespace Dalba.Financeiro.Infrastructure.Configuration;

public class SmtpSettings
{
    public const string SectionName = "Smtp";

    public string Server { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool Ssl { get; set; } = true;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromName { get; set; } = "DALBA Financeiro";
}
