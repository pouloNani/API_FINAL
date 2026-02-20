using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.DTOs.Agent;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.AI.Providers;

public class MistralClient : ILLMClient
{
    public string ProviderName => "Mistral";

    private readonly HttpClient _http;
    private readonly string     _model;

    public MistralClient(HttpClient http, IConfiguration config)
    {
        _http  = http;
        _model = config["AI:Mistral:Model"] ?? "mistral-large-latest";
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", config["AI:Mistral:ApiKey"]);
        _http.Timeout = TimeSpan.FromSeconds(60);
    }

    public async Task<LLMResponse> ChatWithToolsAsync(
        List<MessageDto>     messages,
        List<ToolDefinition> tools,
        string               systemPrompt)
    {
        var payload = new
        {
            model       = _model,
            temperature = 0.3,
            messages    = BuildMessages(messages, systemPrompt),
            tools       = tools.Select(t => new
            {
                type     = "function",
                function = new { name = t.Name, description = t.Description, parameters = t.Parameters }
            }),
            tool_choice = "auto"
        };

        var response = await _http.PostAsJsonAsync(
            "https://api.mistral.ai/v1/chat/completions", payload);
        response.EnsureSuccessStatusCode();

        var raw = await response.Content.ReadFromJsonAsync<MistralRawResponse>();
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
                    content    = (string?)null,
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
                    role         = "tool",
                    tool_call_id = msg.ToolCallId,
                    name         = msg.ToolName,
                    content      = msg.Content ?? ""
                });
            }
            else
            {
                result.Add(new { role = msg.Role, content = msg.Content ?? "" });
            }
        }

        return result;
    }

    private static LLMResponse MapResponse(MistralRawResponse raw)
    {
        var choice  = raw.Choices.First();
        var message = choice.Message;

        if (message.ToolCalls == null || !message.ToolCalls.Any())
            return new LLMResponse { Content = message.Content ?? "", FinishReason = "stop" };

        return new LLMResponse
        {
            Content      = message.Content ?? "",
            FinishReason = "tool_calls",
            ToolCalls    = message.ToolCalls.Select(tc => new ToolCall
            {
                Id           = tc.Id,
                Name         = tc.Function.Name,
                ArgumentsRaw = tc.Function.Arguments,
                Arguments    = JsonSerializer.Deserialize<JsonElement>(tc.Function.Arguments)
            }).ToList()
        };
    }

    // ─── Modèles privés (nested dans la classe) ──────────────────

    private class MistralRawResponse
    {
        [JsonPropertyName("choices")] public List<MistralChoice> Choices { get; set; } = new();
    }

    private class MistralChoice
    {
        [JsonPropertyName("finish_reason")] public string          FinishReason { get; set; } = "";
        [JsonPropertyName("message")]       public MistralMessage  Message      { get; set; } = new();
    }

    private class MistralMessage
    {
        [JsonPropertyName("content")]    public string?                Content   { get; set; }
        [JsonPropertyName("tool_calls")] public List<MistralToolCall>? ToolCalls { get; set; }
    }

    private class MistralToolCall
    {
        [JsonPropertyName("id")]       public string          Id       { get; set; } = "";
        [JsonPropertyName("function")] public MistralFunction Function { get; set; } = new();
    }

    private class MistralFunction
    {
        [JsonPropertyName("name")]      public string Name      { get; set; } = "";
        [JsonPropertyName("arguments")] public string Arguments { get; set; } = "";
    }
}