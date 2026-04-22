using Dalba.Financeiro.Domain.Common;

namespace Dalba.Financeiro.Domain.Entities;

public class ParametroSistema : BaseEntity
{
    public string Chave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; } = true;
}
