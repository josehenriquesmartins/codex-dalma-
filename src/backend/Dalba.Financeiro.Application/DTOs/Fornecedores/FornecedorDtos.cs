using Dalba.Financeiro.Domain.Enums;

namespace Dalba.Financeiro.Application.DTOs.Fornecedores;

public sealed record FornecedorRequest(
    string CodigoFornecedor,
    TipoPessoa TipoPessoa,
    PorteEmpresa? PorteEmpresa,
    long CategoriaId,
    string NomeOuRazaoSocial,
    string? NomeFantasia,
    string CpfOuCnpj,
    string DdiTelefone,
    string DddTelefone,
    string NumeroTelefone,
    string Email,
    string Cep,
    string Logradouro,
    string Numero,
    string? Complemento,
    string Bairro,
    string Cidade,
    string Estado,
    string Pais,
    bool Ativo);

public sealed record FornecedorResponse(
    long Id,
    string CodigoFornecedor,
    TipoPessoa TipoPessoa,
    PorteEmpresa? PorteEmpresa,
    long CategoriaId,
    string CategoriaDescricao,
    string NomeOuRazaoSocial,
    string? NomeFantasia,
    string CpfOuCnpj,
    string DdiTelefone,
    string DddTelefone,
    string NumeroTelefone,
    string Email,
    string Cep,
    string Logradouro,
    string Numero,
    string? Complemento,
    string Bairro,
    string Cidade,
    string Estado,
    string Pais,
    bool Ativo);
