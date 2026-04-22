using Dalba.Financeiro.Domain.Common;

namespace Dalba.Financeiro.Domain.Entities;

public class PasswordResetToken : BaseEntity
{
    public long UsuarioId { get; set; }
    public string TokenHashSha256 { get; set; } = string.Empty;
    public DateTime ExpiraEmUtc { get; set; }
    public DateTime? UtilizadoEmUtc { get; set; }

    public Usuario Usuario { get; set; } = null!;
}
