namespace Dalba.Financeiro.Application.DTOs.Contratos;

public sealed record ContratoRequest(
    long FornecedorId,
    string NumeroContrato,
    string Descricao,
    DateOnly DataInicio,
    DateOnly? DataFim,
    bool Ativo);

public sealed record ContratoResponse(
    long Id,
    long FornecedorId,
    string FornecedorNome,
    string NumeroContrato,
    string Descricao,
    DateOnly DataInicio,
    DateOnly? DataFim,
    bool Ativo,
    bool Vigente);
