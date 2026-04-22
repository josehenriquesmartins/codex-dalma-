namespace Dalba.Financeiro.Domain.Common;

public abstract class BaseEntity
{
    public long Id { get; set; }
    public DateTime DataHoraCriacao { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
    public DateTime? DataHoraAtualizacao { get; set; }
}
