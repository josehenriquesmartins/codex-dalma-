using System.Net;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dalba.Financeiro.Application.Abstractions.Notifications;
using Dalba.Financeiro.Domain.Enums;
using Dalba.Financeiro.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dalba.Financeiro.Infrastructure.Notifications;

public class SmtpNotificationDispatcher : INotificationDispatcher
{
    private readonly SmtpSettings _settings;
    private readonly SmsSettings _smsSettings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SmtpNotificationDispatcher> _logger;

    public SmtpNotificationDispatcher(IOptions<SmtpSettings> options, IOptions<SmsSettings> smsOptions, IHttpClientFactory httpClientFactory, ILogger<SmtpNotificationDispatcher> logger)
    {
        _settings = options.Value;
        _smsSettings = smsOptions.Value;
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
        if (string.IsNullOrWhiteSpace(_settings.Server) ||
            string.IsNullOrWhiteSpace(_settings.User) ||
            string.IsNullOrWhiteSpace(_settings.Password))
        {
            return new NotificationDispatchResult(false, Error: "Configuração SMTP incompleta.");
        }

        try
        {
            using var client = new SmtpClient(_settings.Server, _settings.Port)
            {
                EnableSsl = _settings.Ssl,
                Credentials = new NetworkCredential(_settings.User, _settings.Password)
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.User, _settings.FromName),
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
        if (!string.Equals(_smsSettings.Provider, "COMTELE", StringComparison.OrdinalIgnoreCase))
        {
            return new NotificationDispatchResult(false, Error: "Provedor SMS não configurado.");
        }

        var authKey = string.IsNullOrWhiteSpace(_smsSettings.Token) ? _smsSettings.Account : _smsSettings.Token;
        if (string.IsNullOrWhiteSpace(authKey) || string.IsNullOrWhiteSpace(_smsSettings.Endpoint))
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
            using var request = new HttpRequestMessage(HttpMethod.Post, _smsSettings.Endpoint);
            request.Headers.TryAddWithoutValidation("auth-key", authKey);
            request.Content = JsonContent.Create(new
            {
                Sender = _smsSettings.Sender,
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

    private sealed record ComteleResponse(
        [property: JsonPropertyName("Success")] bool Success,
        [property: JsonPropertyName("Message")] string? Message);
}
