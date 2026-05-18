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
    string? NumeroAf,
    string? NomeOriginalNotaFiscal,
    string? ExtensaoNotaFiscal,
    long? TamanhoBytesNotaFiscal,
    DateTime? DataHoraUploadNotaFiscal,
    string? NomeOriginalBoleto,
    string? ExtensaoBoleto,
    long? TamanhoBytesBoleto,
    DateTime? DataHoraUploadBoleto,
    DateTime DataHoraGeracao);

public sealed record AtualizarFinanceiroRequest(StatusFinanceiro StatusFinanceiro, string? NumeroNotaFiscal, string? Observacao);

public sealed record EnviarNotaFiscalRequest(string NumeroNotaFiscal, string NumeroAf, string? Observacao, IFormFile ArquivoNotaFiscal);

public sealed record EnviarBoletoRequest(string? Observacao, IFormFile ArquivoBoleto);

public sealed record NotaFiscalChecklistItem(string Codigo, string Titulo, bool Ok, string? ValorEncontrado = null);

public sealed record EnvioNotaFiscalResultadoResponse(bool Sucesso, IReadOnlyCollection<NotaFiscalChecklistItem> Checklist);
