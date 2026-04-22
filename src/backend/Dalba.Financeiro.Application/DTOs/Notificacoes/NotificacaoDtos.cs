using Dalba.Financeiro.Domain.Enums;

namespace Dalba.Financeiro.Application.DTOs.Notificacoes;

public sealed record NotificacaoResponse(
    long Id,
    TipoNotificacao TipoNotificacao,
    string Titulo,
    string Mensagem,
    StatusNotificacao StatusEnvio,
    DateTime DataHoraCriacao,
    DateTime? DataHoraEnvio,
    int Tentativas,
    long? UsuarioId,
    long? RemetenteUsuarioId,
    string? Destinatario,
    string? Erro);
