using Dalba.Financeiro.Domain.Enums;

namespace Dalba.Financeiro.Application.DTOs.Admin;

public sealed record ValidarDocumentoRequest(StatusValidacaoDocumento Status, string? ObservacaoAvaliacao);

public sealed record EnvioParaValidacaoDto(
    long Id,
    string Fornecedor,
    string? Contrato,
    short MesReferencia,
    short AnoReferencia,
    StatusEnvioMensal Status,
    DateTime DataHoraRegistro);

public sealed record EnvioValidacaoDetalheDto(
    long Id,
    string Fornecedor,
    string? Contrato,
    short MesReferencia,
    short AnoReferencia,
    StatusEnvioMensal Status,
    DateTime DataHoraRegistro,
    IReadOnlyCollection<DocumentoValidacaoDetalheDto> Documentos);

public sealed record DocumentoValidacaoDetalheDto(
    long Id,
    long DocumentoTipoId,
    string DocumentoNome,
    string NomeOriginalArquivo,
    string CaminhoArquivo,
    string Extensao,
    long TamanhoBytes,
    DateTime DataHoraUpload,
    StatusValidacaoDocumento StatusValidacaoDocumento,
    string? ObservacaoAvaliacao,
    long? AvaliadoPorUsuarioId,
    DateTime? DataHoraAvaliacao);

public sealed record DocumentoVisualizacaoDto(
    string NomeArquivo,
    string ContentType,
    Stream Conteudo);
