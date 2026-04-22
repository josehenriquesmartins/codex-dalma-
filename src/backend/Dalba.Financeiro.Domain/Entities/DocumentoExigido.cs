using Dalba.Financeiro.Domain.Common;
using Dalba.Financeiro.Domain.Enums;

namespace Dalba.Financeiro.Domain.Entities;

public class DocumentoExigido : BaseEntity
{
    public long DocumentoTipoId { get; set; }
    public TipoPessoa TipoPessoa { get; set; }
    public PorteEmpresa? PorteEmpresa { get; set; }
    public long CategoriaId { get; set; }
    public bool Obrigatorio { get; set; } = true;
    public bool Ativo { get; set; } = true;

    public DocumentoTipo? DocumentoTipo { get; set; }
    public Categoria? Categoria { get; set; }
}
