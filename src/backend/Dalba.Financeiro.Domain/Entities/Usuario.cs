using Dalba.Financeiro.Domain.Common;
using Dalba.Financeiro.Domain.Enums;

namespace Dalba.Financeiro.Domain.Entities;

public class Usuario : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string SenhaHashSha256 { get; set; } = string.Empty;
    public PerfilAcesso Perfil { get; set; }
    public long? FornecedorId { get; set; }
    public bool Ativo { get; set; } = true;

    public Fornecedor? Fornecedor { get; set; }
}
