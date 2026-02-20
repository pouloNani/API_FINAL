using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.DTOs.Agent;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.AI.Providers;

public class ScoutClient : ILLMClient
{
    public string ProviderName => "LlamaScout4";

    private readonly HttpClient _http;
    private readonly string     _model;

    public ScoutClient(HttpClient http, IConfiguration config)
    {
        _http  = http;
        _model = config["AI:LlamaScout4:Model"]  ?? "llama-3.3-70b-versatile";
        var apiKey = config["AI:LlamaScout4:ApiKey"] ?? "";
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        _http.Timeout = TimeSpan.FromSeconds(60);
    }

    public async Task<LLMResponse> ChatWithToolsAsync(
        List<MessageDto>     messages,
        List<ToolDefinition> tools,
        string               systemPrompt)
    {
        var msgList = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

        foreach (var m in messages)
        {
            if (m.Role == "tool")
            {
                msgList.Add(new
                {
                    role         = "tool",
                    tool_call_id = m.ToolCallId ?? "",
                    content      = m.Content ?? ""
                });
            }
            else if (m.ToolCalls != null && m.ToolCalls.Any())
            {
                msgList.Add(new
                {
                    role       = m.Role,
                    content    = m.Content ?? "",
                    tool_calls = m.ToolCalls.Select(tc => new
                    {
                        id       = tc.Id,
                        type     = "function",
                        function = new { name = tc.Name, arguments = tc.ArgumentsRaw }
                    })
                });
            }
            else
            {
                msgList.Add(new { role = m.Role, content = m.Content ?? "" });
            }
        }

        var payload = new
        {
            model       = _model,
            messages    = msgList,
            tools       = tools.Select(t => new
            {
                type     = "function",
                function = new
                {
                    name        = t.Name,
                    description = t.Description,
                    parameters  = t.Parameters
                }
            }),
            tool_choice = "auto"
        };

        var response = await _http.PostAsJsonAsync(
                 "https://api.groq.com/openai/v1/chat/completions", payload);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[Groq 400 ERROR] {error}");
                throw new Exception($"Groq API 400: {error}");
            }

            // Plus besoin de EnsureSuccessStatusCode() ici, supprimez-le
            var raw = await response.Content.ReadFromJsonAsync<GroqRawResponse>();
            return MapResponse(raw!);
    }

    private static LLMResponse MapResponse(GroqRawResponse raw)
    {
        var choice  = raw.Choices.First();
        var message = choice.Message;

        if (message.ToolCalls == null || !message.ToolCalls.Any())
            return new LLMResponse { Content = message.Content ?? "", FinishReason = "stop" };

        return new LLMResponse
        {
            Content      = message.Content ?? "",
            FinishReason = "tool_calls",
            ToolCalls    = message.ToolCalls.Select(tc =>
            {
                var argsRaw = tc.Function.Arguments;
                return new ToolCall
                {
                    Id           = tc.Id,
                    Name         = tc.Function.Name,
                    ArgumentsRaw = argsRaw,
                    Arguments    = JsonSerializer.Deserialize<JsonElement>(argsRaw)
                };
            }).ToList()
        };
    }

    private class GroqRawResponse
    {
        [JsonPropertyName("choices")] public List<GroqChoice> Choices { get; set; } = new();
    }

    private class GroqChoice
    {
        [JsonPropertyName("message")]       public GroqMessage Message      { get; set; } = new();
        [JsonPropertyName("finish_reason")] public string      FinishReason { get; set; } = "";
    }

    private class GroqMessage
    {
        [JsonPropertyName("role")]       public string              Role      { get; set; } = "";
        [JsonPropertyName("content")]    public string?             Content   { get; set; }
        [JsonPropertyName("tool_calls")] public List<GroqToolCall>? ToolCalls { get; set; }
    }

    private class GroqToolCall
    {
        [JsonPropertyName("id")]       public string      Id       { get; set; } = "";
        [JsonPropertyName("function")] public GroqFunction Function { get; set; } = new();
    }

    private class GroqFunction
    {
        [JsonPropertyName("name")]      public string Name      { get; set; } = "";
        [JsonPropertyName("arguments")] public string Arguments { get; set; } = "";
    }
}