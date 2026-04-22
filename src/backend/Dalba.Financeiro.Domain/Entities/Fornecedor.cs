using Dalba.Financeiro.Domain.Common;
using Dalba.Financeiro.Domain.Enums;

namespace Dalba.Financeiro.Domain.Entities;

public class Fornecedor : BaseEntity
{
    public string CodigoFornecedor { get; set; } = string.Empty;
    public TipoPessoa TipoPessoa { get; set; }
    public PorteEmpresa? PorteEmpresa { get; set; }
    public long CategoriaId { get; set; }
    public string NomeOuRazaoSocial { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
    public string CpfOuCnpj { get; set; } = string.Empty;
    public string DdiTelefone { get; set; } = string.Empty;
    public string DddTelefone { get; set; } = string.Empty;
    public string NumeroTelefone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;

    public Categoria? Categoria { get; set; }
    public ICollection<Usuario> Usuarios { get; set; } = [];
    public ICollection<Contrato> Contratos { get; set; } = [];
    public ICollection<DocumentoEnviado> DocumentosEnviados { get; set; } = [];
    public ICollection<Notificacao> Notificacoes { get; set; } = [];
    public ICollection<FinanceiroLiberacao> LiberacoesFinanceiras { get; set; } = [];
}
