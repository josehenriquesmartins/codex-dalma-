using Dalba.Financeiro.Application.DTOs.Integrations;

namespace Dalba.Financeiro.Application.Abstractions.Integrations;

public interface IProtheusIntegrationService
{
    Task<ProtheusAfValidationResponse> ValidarAutorizacaoFaturamentoAsync(ProtheusAfValidationRequest request, CancellationToken cancellationToken);
}
