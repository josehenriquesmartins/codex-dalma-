using Dalba.Financeiro.Domain.Common;

namespace Dalba.Financeiro.Domain.Entities;

public class DocumentoTipo : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;
    public string NomeDocumento { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; } = true;

    public ICollection<DocumentoExigido> DocumentosExigidos { get; set; } = [];
    public ICollection<DocumentoRegistrado> DocumentosRegistrados { get; set; } = [];
}
