using Dalba.Financeiro.Domain.Enums;

namespace Dalba.Financeiro.Application.DTOs.Usuarios;

public sealed record UsuarioRequest(
    string Nome,
    string Email,
    string Login,
    string Senha,
    PerfilAcesso Perfil,
    long? FornecedorId,
    bool Ativo);

public sealed record UsuarioResponse(
    long Id,
    string Nome,
    string Email,
    string Login,
    PerfilAcesso Perfil,
    long? FornecedorId,
    bool Ativo,
    DateTime DataHoraCriacao);
