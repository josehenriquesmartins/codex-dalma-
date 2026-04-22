using Dalba.Financeiro.Domain.Enums;

namespace Dalba.Financeiro.Application.Abstractions.Notifications;

public interface INotificationDispatcher
{
    Task<NotificationDispatchResult> DispatchAsync(TipoNotificacao tipo, string destination, string title, string message, CancellationToken cancellationToken);
}

public sealed record NotificationDispatchResult(bool Success, string? ProviderMessage = null, string? Error = null);
