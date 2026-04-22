using Dalba.Financeiro.Domain.Common;
using Dalba.Financeiro.Domain.Enums;

namespace Dalba.Financeiro.Domain.Entities;

public class DocumentoEnviado : BaseEntity
{
    public long FornecedorId { get; set; }
    public long UsuarioId { get; set; }
    public long ContratoId { get; set; }
    public short MesReferencia { get; set; }
    public short AnoReferencia { get; set; }
    public DateTime DataHoraRegistro { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
    public StatusEnvioMensal Status { get; set; } = StatusEnvioMensal.Pendente;
    public string UsuarioRegistro { get; set; } = string.Empty;
    public string? Observacao { get; set; }
    public long? AvaliadoPorUsuarioId { get; set; }
    public DateTime? DataHoraValidacaoFinal { get; set; }

    public Fornecedor? Fornecedor { get; set; }
    public Usuario? Usuario { get; set; }
    public Usuario? AvaliadoPorUsuario { get; set; }
    public Contrato? Contrato { get; set; }
    public ICollection<DocumentoRegistrado> DocumentosRegistrados { get; set; } = [];
    public ICollection<FinanceiroLiberacao> LiberacoesFinanceiras { get; set; } = [];
}
