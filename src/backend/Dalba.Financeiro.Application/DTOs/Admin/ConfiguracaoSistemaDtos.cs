namespace Dalba.Financeiro.Application.DTOs.Admin;

public sealed record ConfiguracaoSistemaResponse(
    string? SmtpHost,
    string? SmtpPorta,
    string? SmtpUsuario,
    string? SmtpSenha,
    string? SmsProvider,
    string? SmsConta,
    string? SmsToken,
    string? SmsRemetente,
    string? IaApiKey,
    string? WhatsAppApiKey);

public sealed record SalvarConfiguracaoSistemaRequest(
    string? SmtpHost,
    string? SmtpPorta,
    string? SmtpUsuario,
    string? SmtpSenha,
    string? SmsProvider,
    string? SmsConta,
    string? SmsToken,
    string? SmsRemetente,
    string? IaApiKey,
    string? WhatsAppApiKey);
