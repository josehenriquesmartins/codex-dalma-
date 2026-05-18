namespace Dalba.Financeiro.Infrastructure.Configuration;

public class ProtheusSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool Enabled { get; set; }
}
