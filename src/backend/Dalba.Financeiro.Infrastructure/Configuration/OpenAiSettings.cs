namespace Dalba.Financeiro.Infrastructure.Configuration;

public class OpenAiSettings
{
    public const string SectionName = "OpenAi";

    public string Endpoint { get; set; } = "https://api.openai.com/v1/chat/completions";
    public string Model { get; set; } = "gpt-4o";
    public int TimeoutSeconds { get; set; } = 60;
}
