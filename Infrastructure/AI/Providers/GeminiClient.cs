using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.DTOs.Agent;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.AI.Providers;

public class GeminiClient : ILLMClient
{
    public string ProviderName => "Gemini";

    private readonly HttpClient _http;
    private readonly string     _model;
    private readonly string     _apiKey;

    public GeminiClient(HttpClient http, IConfiguration config)
    {
        _http   = http;
        _model  = config["AI:Gemini:Model"]  ?? "gemini-1.5-flash";
        _apiKey = config["AI:Gemini:ApiKey"] ?? "";
        _http.Timeout = TimeSpan.FromSeconds(60);
    }

    public async Task<LLMResponse> ChatWithToolsAsync(
        List<MessageDto>     messages,
        List<ToolDefinition> tools,
        string               systemPrompt)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

        var payload = new
        {
            system_instruction = new { parts = new[] { new { text = systemPrompt } } },
            contents = messages.Select(m => new
            {
                role  = m.Role == "assistant" ? "model" : "user",
                parts = new[] { new { text = m.Content ?? "" } }
            }),
            tools = new[]
            {
                new
                {
                    function_declarations = tools.Select(t => new
                    {
                        name        = t.Name,
                        description = t.Description,
                        parameters  = t.Parameters
                    })
                }
            }
        };

        var response = await _http.PostAsJsonAsync(url, payload);
        response.EnsureSuccessStatusCode();

        var raw = await response.Content.ReadFromJsonAsync<GeminiRawResponse>();
        return MapResponse(raw!);
    }

    private static LLMResponse MapResponse(GeminiRawResponse raw)
    {
        var candidate = raw.Candidates.First();
        var parts     = candidate.Content.Parts;

        // Cherche un function call parmi tous les parts
        var functionCall = parts.FirstOrDefault(p => p.FunctionCall != null)?.FunctionCall;

        if (functionCall != null)
        {
            var argsRaw = JsonSerializer.Serialize(functionCall.Args);
            return new LLMResponse
            {
                FinishReason = "tool_calls",
                ToolCalls    = new List<ToolCall>
                {
                    new()
                    {
                        Id           = Guid.NewGuid().ToString(),
                        Name         = functionCall.Name,
                        ArgumentsRaw = argsRaw,
                        Arguments    = JsonSerializer.Deserialize<JsonElement>(argsRaw)
                    }
                }
            };
        }

        // Concatène tous les texts
        var text = string.Join("", parts.Select(p => p.Text ?? ""));
        return new LLMResponse { Content = text, FinishReason = "stop" };
    }

    // ─── Modèles privés (nested dans la classe) ──────────────────

    private class GeminiRawResponse
    {
        [JsonPropertyName("candidates")] public List<GeminiCandidate> Candidates { get; set; } = new();
    }

    private class GeminiCandidate
    {
        [JsonPropertyName("content")] public GeminiContent Content { get; set; } = new();
    }

    private class GeminiContent
    {
        [JsonPropertyName("parts")] public List<GeminiPart> Parts { get; set; } = new();
    }

    private class GeminiPart
    {
        [JsonPropertyName("text")]         public string?             Text         { get; set; }
        [JsonPropertyName("functionCall")] public GeminiFunctionCall? FunctionCall { get; set; }
    }

    private class GeminiFunctionCall
    {
        [JsonPropertyName("name")] public string      Name { get; set; } = "";
        [JsonPropertyName("args")] public JsonElement Args { get; set; }
    }
}