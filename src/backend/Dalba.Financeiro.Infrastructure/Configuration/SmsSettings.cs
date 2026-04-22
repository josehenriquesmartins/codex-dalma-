namespace Dalba.Financeiro.Infrastructure.Configuration;

public class SmsSettings
{
    public const string SectionName = "Sms";

    public string Provider { get; set; } = string.Empty;
    public string Account { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public string Endpoint { get; set; } = "https://sms.comtele.com.br/api/v2/send";
}
