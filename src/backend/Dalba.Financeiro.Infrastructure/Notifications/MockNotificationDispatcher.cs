using Dalba.Financeiro.Application.Abstractions.Notifications;
using Dalba.Financeiro.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Dalba.Financeiro.Infrastructure.Notifications;

public class MockNotificationDispatcher : INotificationDispatcher
{
    private readonly ILogger<MockNotificationDispatcher> _logger;

    public MockNotificationDispatcher(ILogger<MockNotificationDispatcher> logger)
    {
        _logger = logger;
    }

    public Task<NotificationDispatchResult> DispatchAsync(TipoNotificacao tipo, string destination, string title, string message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Mock {Tipo} enviado para {Destino}. Título: {Titulo}. Mensagem: {Mensagem}", tipo, destination, title, message);
        return Task.FromResult(new NotificationDispatchResult(true, "Mock provider success"));
    }
}
