using Dalba.Financeiro.Domain.Enums;

namespace Dalba.Financeiro.Application.DTOs.Documentos;

public sealed record DocumentoTipoRequest(string Codigo, string NomeDocumento, string? Descricao, bool Ativo);
public sealed record DocumentoTipoResponse(long Id, string Codigo, string NomeDocumento, string? Descricao, bool Ativo);

public sealed record DocumentoExigidoRequest(
    long DocumentoTipoId,
    TipoPessoa TipoPessoa,
    PorteEmpresa? PorteEmpresa,
    long CategoriaId,
    bool Obrigatorio,
    bool Ativo);

public sealed record DocumentoExigidoResponse(
    long Id,
    long DocumentoTipoId,
    string DocumentoNome,
    TipoPessoa TipoPessoa,
    PorteEmpresa? PorteEmpresa,
    long CategoriaId,
    string CategoriaDescricao,
    bool Obrigatorio,
    bool Ativo);
