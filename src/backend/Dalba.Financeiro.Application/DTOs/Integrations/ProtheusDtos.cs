namespace Dalba.Financeiro.Application.DTOs.Integrations;

public sealed record ProtheusAfValidationRequest(
    string NumeroAf,
    string CnpjFornecedor,
    decimal ValorDocumento,
    string? ChaveAcesso,
    string TipoDocumento);

public sealed record ProtheusAfValidationResponse(
    bool Valido,
    string Mensagem,
    string? NumeroAf,
    string? CnpjAf,
    decimal? SaldoAf);
