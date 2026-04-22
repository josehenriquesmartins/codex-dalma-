using Dalba.Financeiro.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Dalba.Financeiro.Application.DTOs.Financeiro;

public sealed record FinanceiroLiberacaoResponse(
    long Id,
    long DocumentoEnviadoId,
    string Fornecedor,
    string? Contrato,
    short MesReferencia,
    short AnoReferencia,
    StatusFinanceiro StatusFinanceiro,
    string? NumeroNotaFiscal,
    string? NomeOriginalNotaFiscal,
    string? ExtensaoNotaFiscal,
    long? TamanhoBytesNotaFiscal,
    DateTime? DataHoraUploadNotaFiscal,
    DateTime DataHoraGeracao);

public sealed record AtualizarFinanceiroRequest(StatusFinanceiro StatusFinanceiro, string? NumeroNotaFiscal, string? Observacao);

public sealed record EnviarNotaFiscalRequest(string NumeroNotaFiscal, string? Observacao, IFormFile ArquivoNotaFiscal);
