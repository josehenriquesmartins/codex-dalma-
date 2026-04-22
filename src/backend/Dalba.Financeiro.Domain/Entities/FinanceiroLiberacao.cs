using Dalba.Financeiro.Domain.Common;
using Dalba.Financeiro.Domain.Enums;

namespace Dalba.Financeiro.Domain.Entities;

public class FinanceiroLiberacao : BaseEntity
{
    public long DocumentoEnviadoId { get; set; }
    public long FornecedorId { get; set; }
    public long? ContratoId { get; set; }
    public StatusFinanceiro StatusFinanceiro { get; set; } = StatusFinanceiro.AguardandoEnvioNf;
    public DateTime DataHoraGeracao { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
    public long GeradoPorUsuarioId { get; set; }
    public string? Observacao { get; set; }
    public string? NumeroNotaFiscal { get; set; }
    public string? NomeOriginalNotaFiscal { get; set; }
    public string? NomeArquivoFisicoNotaFiscal { get; set; }
    public string? CaminhoArquivoNotaFiscal { get; set; }
    public string? ExtensaoNotaFiscal { get; set; }
    public long? TamanhoBytesNotaFiscal { get; set; }
    public DateTime? DataRecebimentoNotaFiscal { get; set; }
    public DateTime? DataHoraUploadNotaFiscal { get; set; }

    public DocumentoEnviado? DocumentoEnviado { get; set; }
    public Fornecedor? Fornecedor { get; set; }
    public Contrato? Contrato { get; set; }
    public Usuario? GeradoPorUsuario { get; set; }
}
