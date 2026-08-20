namespace Dalba.Financeiro.Application.Abstractions.Ia;

public sealed record IaDocumentoContexto(
    string TipoDocumento,
    string NomeFornecedor,
    string CpfOuCnpj,
    int MesReferencia,
    int AnoReferencia);

public sealed record IaEntradaDocumento(
    string? TextoExtraido,
    string? ImagemBase64,
    string? ImagemMediaType);

public sealed record IaAnaliseResultado(
    string Sugestao,
    bool TipoConfere,
    bool VigenciaOk,
    bool DadosConferem,
    string Justificativa,
    string Provider);

/// <summary>
/// Analisa um documento enviado por fornecedor e devolve uma SUGESTÃO para conferência humana.
/// A chave de API é recebida por parâmetro: quem chama só aciona a IA quando a chave está configurada.
/// </summary>
public interface IIaDocumentAnalyzer
{
    string Provider { get; }

    Task<IaAnaliseResultado> AnalisarAsync(
        IaEntradaDocumento entrada,
        IaDocumentoContexto contexto,
        string apiKey,
        CancellationToken ct);
}
