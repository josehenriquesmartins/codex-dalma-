using Dalba.Financeiro.Domain.Common;
using Dalba.Financeiro.Domain.Enums;

namespace Dalba.Financeiro.Domain.Entities;

public class Notificacao : BaseEntity
{
    public long? UsuarioId { get; set; }
    public long? RemetenteUsuarioId { get; set; }
    public long? FornecedorId { get; set; }
    public TipoNotificacao TipoNotificacao { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public StatusNotificacao StatusEnvio { get; set; } = StatusNotificacao.Pendente;
    public DateTime? DataHoraEnvio { get; set; }
    public int Tentativas { get; set; }
    public string? ReferenciaEntidade { get; set; }
    public long? ReferenciaId { get; set; }
    public string? Destinatario { get; set; }
    public string? Erro { get; set; }

    public Usuario? Usuario { get; set; }
    public Usuario? RemetenteUsuario { get; set; }
    public Fornecedor? Fornecedor { get; set; }
}
