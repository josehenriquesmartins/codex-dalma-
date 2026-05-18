using Dalba.Financeiro.Application.Abstractions.Integrations;
using Dalba.Financeiro.Application.DTOs.Integrations;
using Dalba.Financeiro.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Dalba.Financeiro.Infrastructure.Integrations;

public class MockProtheusIntegrationService : IProtheusIntegrationService
{
    private readonly ProtheusSettings _settings;

    public MockProtheusIntegrationService(IOptions<ProtheusSettings> settings)
    {
        _settings = settings.Value;
    }

    public Task<ProtheusAfValidationResponse> ValidarAutorizacaoFaturamentoAsync(ProtheusAfValidationRequest request, CancellationToken cancellationToken)
    {
        if (!_settings.Enabled)
        {
            return Task.FromResult(new ProtheusAfValidationResponse(
                true,
                "Integração Protheus preparada em modo simulado. Ative Protheus:Enabled após validação dos endpoints com Marcelo Nunes e/ou Sergio Silva.",
                request.NumeroAf,
                request.CnpjFornecedor,
                request.ValorDocumento));
        }

        return Task.FromResult(new ProtheusAfValidationResponse(
            false,
            "Protheus habilitado, mas o cliente HTTP real ainda depende da confirmação dos endpoints oficiais.",
            request.NumeroAf,
            null,
            null));
    }
}
