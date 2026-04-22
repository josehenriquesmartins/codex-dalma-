using Dalba.Financeiro.Domain.Common;
using Dalba.Financeiro.Domain.Enums;

namespace Dalba.Financeiro.Domain.Entities;

public class DocumentoRegistrado : BaseEntity
{
    public long DocumentoEnviadoId { get; set; }
    public long DocumentoTipoId { get; set; }
    public string NomeOriginalArquivo { get; set; } = string.Empty;
    public string NomeArquivoFisico { get; set; } = string.Empty;
    public string CaminhoArquivo { get; set; } = string.Empty;
    public string Extensao { get; set; } = string.Empty;
    public long TamanhoBytes { get; set; }
    public DateTime DataHoraUpload { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
    public string UsuarioUpload { get; set; } = string.Empty;
    public StatusValidacaoDocumento StatusValidacaoDocumento { get; set; } = StatusValidacaoDocumento.Pendente;
    public long? AvaliadoPorUsuarioId { get; set; }
    public DateTime? DataHoraAvaliacao { get; set; }
    public string? ObservacaoAvaliacao { get; set; }

    public DocumentoEnviado? DocumentoEnviado { get; set; }
    public DocumentoTipo? DocumentoTipo { get; set; }
    public Usuario? AvaliadoPorUsuario { get; set; }
}
