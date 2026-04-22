using Dalba.Financeiro.Domain.Enums;

namespace Dalba.Financeiro.Application.Abstractions.Audit;

public interface IAuditService
{
    Task RegistrarAsync(string entidade, long? entidadeId, AcaoAuditoria acao, string dadosResumidos, CancellationToken cancellationToken);
}
