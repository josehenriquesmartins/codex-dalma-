using Dalba.Financeiro.Domain.Common;

namespace Dalba.Financeiro.Domain.Entities;

public class Categoria : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;

    public ICollection<Fornecedor> Fornecedores { get; set; } = [];
    public ICollection<DocumentoExigido> DocumentosExigidos { get; set; } = [];
}
