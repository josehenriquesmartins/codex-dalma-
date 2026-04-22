using Dalba.Financeiro.Domain.Common;
using Dalba.Financeiro.Domain.Enums;

namespace Dalba.Financeiro.Domain.Entities;

public class LogAuditoria : BaseEntity
{
    public long? UsuarioId { get; set; }
    public string Entidade { get; set; } = string.Empty;
    public long? EntidadeId { get; set; }
    public AcaoAuditoria Acao { get; set; }
    public string DadosResumidos { get; set; } = string.Empty;
    public string? IpOrigem { get; set; }

    public Usuario? Usuario { get; set; }
}
