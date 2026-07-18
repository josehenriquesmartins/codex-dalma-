using System.Net;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dalba.Financeiro.Application.Abstractions.Notifications;
using Dalba.Financeiro.Application.Abstractions.Persistence;
using Dalba.Financeiro.Domain.Enums;
using Dalba.Financeiro.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dalba.Financeiro.Infrastructure.Notifications;

public class SmtpNotificationDispatcher : INotificationDispatcher
{
    private readonly SmtpSettings _settings;
    private readonly SmsSettings _smsSettings;
    private readonly IAppDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SmtpNotificationDispatcher> _logger;

    public SmtpNotificationDispatcher(
        IOptions<SmtpSettings> options,
        IOptions<SmsSettings> smsOptions,
        IAppDbContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<SmtpNotificationDispatcher> logger)
    {
        _settings = options.Value;
        _smsSettings = smsOptions.Value;
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<NotificationDispatchResult> DispatchAsync(TipoNotificacao tipo, string destination, string title, string message, CancellationToken cancellationToken)
    {
        if (tipo == TipoNotificacao.Email)
        {
            return await DispatchEmailAsync(destination, title, message, cancellationToken);
        }

        if (tipo == TipoNotificacao.Sms)
        {
            return await DispatchSmsAsync(destination, message, cancellationToken);
        }

        _logger.LogInformation("{Tipo} registrado sem envio externo para {Destino}. Título: {Titulo}", tipo, destination, title);
        return new NotificationDispatchResult(true, "Notificação sem envio externo");
    }

    private async Task<NotificationDispatchResult> DispatchEmailAsync(string destination, string title, string message, CancellationToken cancellationToken)
    {
        var settings = await ResolveSmtpSettingsAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(settings.Server) ||
            string.IsNullOrWhiteSpace(settings.User) ||
            string.IsNullOrWhiteSpace(settings.Password))
        {
            return new NotificationDispatchResult(false, Error: "Configuração SMTP incompleta.");
        }

        try
        {
            using var client = new SmtpClient(settings.Server, settings.Port)
            {
                EnableSsl = settings.Ssl,
                Credentials = new NetworkCredential(settings.User, settings.Password)
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(settings.User, settings.FromName),
                Subject = title,
                Body = message,
                IsBodyHtml = false
            };

            mailMessage.To.Add(destination);
            await client.SendMailAsync(mailMessage, cancellationToken);
            return new NotificationDispatchResult(true, "E-mail enviado por SMTP");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao enviar e-mail para {Destino}.", destination);
            return new NotificationDispatchResult(false, Error: ex.Message);
        }
    }

    private async Task<NotificationDispatchResult> DispatchSmsAsync(string destination, string message, CancellationToken cancellationToken)
    {
        var settings = await ResolveSmsSettingsAsync(cancellationToken);

        if (!string.Equals(settings.Provider, "COMTELE", StringComparison.OrdinalIgnoreCase))
        {
            return new NotificationDispatchResult(false, Error: "Provedor SMS não configurado.");
        }

        var authKey = string.IsNullOrWhiteSpace(settings.Token) ? settings.Account : settings.Token;
        if (string.IsNullOrWhiteSpace(authKey) || string.IsNullOrWhiteSpace(settings.Endpoint))
        {
            return new NotificationDispatchResult(false, Error: "Configuração SMS incompleta.");
        }

        var phone = NormalizePhone(destination);
        if (string.IsNullOrWhiteSpace(phone))
        {
            return new NotificationDispatchResult(false, Error: "Telefone de SMS inválido.");
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, settings.Endpoint);
            request.Headers.TryAddWithoutValidation("auth-key", authKey);
            request.Content = JsonContent.Create(new
            {
                Sender = settings.Sender,
                Receivers = phone,
                Content = message
            });

            using var response = await client.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new NotificationDispatchResult(false, Error: $"Comtele HTTP {(int)response.StatusCode}: {body}");
            }

            var result = JsonSerializer.Deserialize<ComteleResponse>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result?.Success == true
                ? new NotificationDispatchResult(true, result.Message)
                : new NotificationDispatchResult(false, Error: result?.Message ?? $"Retorno SMS inválido: {body}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao enviar SMS para {Destino}.", destination);
            return new NotificationDispatchResult(false, Error: ex.Message);
        }
    }

    private static string NormalizePhone(string value)
    {
        var digits = new string(value.Where(char.IsDigit).ToArray());
        return digits.StartsWith("55") && digits.Length > 11 ? digits[2..] : digits;
    }

    private async Task<SmtpSettings> ResolveSmtpSettingsAsync(CancellationToken cancellationToken)
    {
        var parametros = await LoadParametrosAsync(SmtpKeys, cancellationToken);

        return new SmtpSettings
        {
            Server = FirstValue(parametros, CfgSmtpHost) ?? _settings.Server,
            Port = ParseInt(FirstValue(parametros, CfgSmtpPorta), _settings.Port),
            Ssl = _settings.Ssl,
            User = FirstValue(parametros, CfgSmtpUsuario) ?? _settings.User,
            Password = FirstValue(parametros, CfgSmtpSenha) ?? _settings.Password,
            FromName = _settings.FromName
        };
    }

    private async Task<SmsSettings> ResolveSmsSettingsAsync(CancellationToken cancellationToken)
    {
        var parametros = await LoadParametrosAsync(SmsKeys, cancellationToken);

        return new SmsSettings
        {
            Provider = FirstValue(parametros, CfgSmsProvider) ?? _smsSettings.Provider,
            Account = FirstValue(parametros, CfgSmsConta) ?? _smsSettings.Account,
            Token = FirstValue(parametros, CfgSmsToken) ?? _smsSettings.Token,
            Password = FirstValue(parametros, CfgSmsSenha) ?? _smsSettings.Password,
            Sender = FirstValue(parametros, CfgSmsRemetente) ?? _smsSettings.Sender,
            Endpoint = FirstValue(parametros, CfgSmsEndpoint) ?? _smsSettings.Endpoint
        };
    }

    private async Task<Dictionary<string, string>> LoadParametrosAsync(IEnumerable<string> keys, CancellationToken cancellationToken)
    {
        var keyList = keys.ToArray();
        return await _context.ParametrosSistema
            .AsNoTracking()
            .Where(x => x.Ativo && keyList.Contains(x.Chave))
            .ToDictionaryAsync(x => x.Chave, x => x.Valor, cancellationToken);
    }

    private static string? FirstValue(IReadOnlyDictionary<string, string> parametros, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (parametros.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }

    private static int ParseInt(string? value, int fallback) =>
        int.TryParse(value, out var parsed) && parsed > 0 ? parsed : fallback;

    private sealed record ComteleResponse(
        [property: JsonPropertyName("Success")] bool Success,
        [property: JsonPropertyName("Message")] string? Message);

    private const string CfgSmtpHost = "CFG_SMTP_HOST";
    private const string CfgSmtpPorta = "CFG_SMTP_PORTA";
    private const string CfgSmtpUsuario = "CFG_SMTP_USUARIO";
    private const string CfgSmtpSenha = "CFG_SMTP_SENHA";
    private const string CfgSmsProvider = "CFG_SMS_PROVIDER";
    private const string CfgSmsConta = "CFG_SMS_CONTA";
    private const string CfgSmsToken = "CFG_SMS_TOKEN";
    private const string CfgSmsRemetente = "CFG_SMS_REMETENTE";
    private const string CfgSmsSenha = "CFG_SMS_SENHA";
    private const string CfgSmsEndpoint = "CFG_SMS_ENDPOINT";

    private static readonly string[] SmtpKeys =
    [
        CfgSmtpHost,
        CfgSmtpPorta,
        CfgSmtpUsuario,
        CfgSmtpSenha
    ];

    private static readonly string[] SmsKeys =
    [
        CfgSmsProvider,
        CfgSmsConta,
        CfgSmsToken,
        CfgSmsRemetente,
        CfgSmsSenha,
        CfgSmsEndpoint
    ];
}
