using Dalba.Financeiro.Domain.Enums;

namespace Dalba.Financeiro.Application.DTOs.Portal;

public sealed record DocumentoObrigatorioDto(
    long DocumentoTipoId,
    long? DocumentoRegistradoId,
    string Codigo,
    string NomeDocumento,
    bool Obrigatorio,
    bool Enviado,
    StatusValidacaoDocumento? StatusValidacao,
    string? NomeOriginalArquivo,
    string? Extensao,
    long? TamanhoBytes,
    DateTime? DataHoraUpload);

public sealed record CriarEnvioMensalRequest(short MesReferencia, short AnoReferencia, long? ContratoId, string? Observacao);

public sealed record EnvioMensalResponse(
    long Id,
    long FornecedorId,
    long UsuarioId,
    long? ContratoId,
    string? ContratoNumero,
    string? ContratoDescricao,
    short MesReferencia,
    short AnoReferencia,
    StatusEnvioMensal Status,
    string? Observacao,
    string? Mensagem,
    IReadOnlyCollection<DocumentoObrigatorioDto> Documentos);

public sealed record UploadDocumentoResponse(long DocumentoRegistradoId, long DocumentoEnviadoId, StatusEnvioMensal StatusEnvio);
