using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Dalba.Financeiro.Application.Abstractions.Ia;
using Dalba.Financeiro.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Dalba.Financeiro.Infrastructure.Integrations;

public class OpenAiDocumentAnalyzer : IIaDocumentAnalyzer
{
    private const string SystemPrompt =
        "Você é um assistente que confere documentos fiscais e trabalhistas enviados por fornecedores. " +
        "Verifique três pontos: (1) se o arquivo é realmente o TIPO de documento esperado; " +
        "(2) se está VIGENTE / dentro do prazo e se a competência corresponde à informada; " +
        "(3) se o CPF/CNPJ e o nome/razão social CONFEREM com o cadastro informado. " +
        "Responda SOMENTE com um objeto JSON com as chaves: " +
        "\"tipoConfere\" (boolean), \"vigenciaOk\" (boolean), \"dadosConferem\" (boolean), " +
        "\"sugestao\" (\"Aprovar\", \"Revisar\" ou \"Reprovar\") e " +
        "\"justificativa\" (string curta em português explicando a decisão). " +
        "Use \"Aprovar\" apenas quando as três verificações passarem; \"Reprovar\" quando o tipo estiver " +
        "claramente errado ou o documento vencido; caso contrário \"Revisar\". " +
        "Sua resposta é apenas uma sugestão para conferência humana, nunca uma decisão final.";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OpenAiSettings _settings;

    public OpenAiDocumentAnalyzer(IHttpClientFactory httpClientFactory, IOptions<OpenAiSettings> settings)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
    }

    public string Provider => "OpenAI";

    public async Task<IaAnaliseResultado> AnalisarAsync(IaEntradaDocumento entrada, IaDocumentoContexto contexto, string apiKey, CancellationToken ct)
    {
        var userText =
            $"Documento esperado: {contexto.TipoDocumento}\n" +
            $"Fornecedor cadastrado: {contexto.NomeFornecedor}\n" +
            $"CPF/CNPJ cadastrado: {contexto.CpfOuCnpj}\n" +
            $"Competência: {contexto.MesReferencia:00}/{contexto.AnoReferencia}\n\n" +
            "Analise o documento abaixo e responda apenas em JSON.";

        var content = new List<object> { new { type = "text", text = userText } };
        if (!string.IsNullOrWhiteSpace(entrada.TextoExtraido))
        {
            content.Add(new { type = "text", text = "Conteúdo extraído do documento:\n" + entrada.TextoExtraido });
        }
        else if (!string.IsNullOrWhiteSpace(entrada.ImagemBase64))
        {
            content.Add(new { type = "image_url", image_url = new { url = $"data:{entrada.ImagemMediaType};base64,{entrada.ImagemBase64}" } });
        }
        else
        {
            throw new InvalidOperationException("Não foi possível ler o conteúdo do documento para análise.");
        }

        var payload = new
        {
            model = _settings.Model,
            temperature = 0,
            response_format = new { type = "json_object" },
            messages = new object[]
            {
                new { role = "system", content = SystemPrompt },
                new { role = "user", content = content }
            }
        };

        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);

        using var request = new HttpRequestMessage(HttpMethod.Post, _settings.Endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(payload);

        using var response = await client.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Falha na API da OpenAI ({(int)response.StatusCode}). {Resumir(body)}");
        }

        using var envelope = JsonDocument.Parse(body);
        var contentText = envelope.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "{}";

        using var parsed = JsonDocument.Parse(contentText);
        var root = parsed.RootElement;

        return new IaAnaliseResultado(
            NormalizarSugestao(GetString(root, "sugestao")),
            GetBool(root, "tipoConfere"),
            GetBool(root, "vigenciaOk"),
            GetBool(root, "dadosConferem"),
            GetString(root, "justificativa") ?? string.Empty,
            Provider);
    }

    private static string NormalizarSugestao(string? valor)
    {
        var v = (valor ?? string.Empty).Trim().ToLowerInvariant();
        return v switch
        {
            "aprovar" => "Aprovar",
            "reprovar" => "Reprovar",
            _ => "Revisar"
        };
    }

    private static string? GetString(JsonElement root, string prop) =>
        root.TryGetProperty(prop, out var el) && el.ValueKind == JsonValueKind.String ? el.GetString() : null;

    private static bool GetBool(JsonElement root, string prop) =>
        root.TryGetProperty(prop, out var el) && el.ValueKind is JsonValueKind.True or JsonValueKind.False && el.GetBoolean();

    private static string Resumir(string body) =>
        string.IsNullOrWhiteSpace(body) ? string.Empty : (body.Length > 300 ? body[..300] : body);
}
