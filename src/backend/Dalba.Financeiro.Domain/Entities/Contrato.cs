using Dalba.Financeiro.Domain.Common;

namespace Dalba.Financeiro.Domain.Entities;

public class Contrato : BaseEntity
{
    public long FornecedorId { get; set; }
    public string NumeroContrato { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateOnly DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public bool Ativo { get; set; } = true;

    public Fornecedor? Fornecedor { get; set; }
    public ICollection<DocumentoEnviado> DocumentosEnviados { get; set; } = [];
    public ICollection<FinanceiroLiberacao> LiberacoesFinanceiras { get; set; } = [];
}
