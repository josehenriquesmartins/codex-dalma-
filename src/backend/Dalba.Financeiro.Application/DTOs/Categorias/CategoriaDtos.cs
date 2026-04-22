namespace Dalba.Financeiro.Application.DTOs.Categorias;

public sealed record CategoriaRequest(string Codigo, string Descricao, bool Ativo);

public sealed record CategoriaResponse(long Id, string Codigo, string Descricao, bool Ativo);
