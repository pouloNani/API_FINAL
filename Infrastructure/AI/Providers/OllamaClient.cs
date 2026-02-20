using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.DTOs.Agent;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.AI.Providers;

public class OllamaClient : ILLMClient
{
    public string ProviderName => "Ollama";

    private readonly HttpClient _http;
    private readonly string     _model;

    public OllamaClient(HttpClient http, IConfiguration config)
    {
        _http             = http;
        _model            = config["AI:Ollama:Model"] ?? "mistral";
        _http.BaseAddress = new Uri(config["AI:Ollama:BaseUrl"] ?? "http://localhost:11434");
        _http.Timeout     = TimeSpan.FromSeconds(120);
    }

    public async Task<LLMResponse> ChatWithToolsAsync(
        List<MessageDto>     messages,
        List<ToolDefinition> tools,
        string               systemPrompt)
    {
        var payload = new
        {
            model    = _model,
            stream   = false,
            messages = BuildMessages(messages, systemPrompt),
            tools    = tools.Select(t => new
            {
                type     = "function",
                function = new
                {
                    name        = t.Name,
                    description = t.Description,
                    parameters  = t.Parameters
                }
            })
        };

        var response = await _http.PostAsJsonAsync("/api/chat", payload);
        response.EnsureSuccessStatusCode();

        var raw = await response.Content.ReadFromJsonAsync<OllamaRawResponse>();
        return MapResponse(raw!);
    }

    private static List<object> BuildMessages(List<MessageDto> messages, string systemPrompt)
    {
        var result = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

        foreach (var msg in messages)
        {
            if (msg.ToolCalls != null && msg.ToolCalls.Any())
            {
                result.Add(new
                {
                    role       = "assistant",
                    content    = msg.Content ?? "",
                    tool_calls = msg.ToolCalls.Select(tc => new
                    {
                        id       = tc.Id,
                        type     = "function",
                        function = new { name = tc.Name, arguments = tc.ArgumentsRaw }
                    })
                });
            }
            else if (msg.Role == "tool")
            {
                result.Add(new
                {
                    role    = "tool",
                    content = msg.Content ?? ""
                });
            }
            else
            {
                result.Add(new { role = msg.Role, content = msg.Content ?? "" });
            }
        }

        return result;
    }

    private static LLMResponse MapResponse(OllamaRawResponse raw)
    {
        var msg = raw.Message;

        if (msg.ToolCalls == null || !msg.ToolCalls.Any())
            return new LLMResponse { Content = msg.Content ?? "", FinishReason = "stop" };

        return new LLMResponse
        {
            Content      = msg.Content ?? "",
            FinishReason = "tool_calls",
            ToolCalls    = msg.ToolCalls.Select(tc =>
            {
                var argsRaw = JsonSerializer.Serialize(tc.Function.Arguments);
                return new ToolCall
                {
                    Id           = Guid.NewGuid().ToString(),
                    Name         = tc.Function.Name,
                    ArgumentsRaw = argsRaw,
                    Arguments    = JsonSerializer.Deserialize<JsonElement>(argsRaw)
                };
            }).ToList()
        };
    }

    // ─── Modèles privés (nested dans la classe) ──────────────────

    private class OllamaRawResponse
    {
        [JsonPropertyName("message")] public OllamaMessage Message { get; set; } = new();
    }

    private class OllamaMessage
    {
        [JsonPropertyName("content")]    public string?               Content   { get; set; }
        [JsonPropertyName("tool_calls")] public List<OllamaToolCall>? ToolCalls { get; set; }
    }

    private class OllamaToolCall
    {
        [JsonPropertyName("function")] public OllamaFunction Function { get; set; } = new();
    }

    private class OllamaFunction
    {
        [JsonPropertyName("name")]      public string      Name      { get; set; } = "";
        [JsonPropertyName("arguments")] public JsonElement Arguments { get; set; }
    }
}