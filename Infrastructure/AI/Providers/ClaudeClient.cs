using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.DTOs.Agent;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.AI.Providers;

public class ClaudeAIClient : ILLMClient
{
    public string ProviderName => "Claude";

    private readonly HttpClient _http;
    private readonly string     _model;

    public ClaudeAIClient(HttpClient http, IConfiguration config)
    {
        _http  = http;
        _model = config["AI:Claude:Model"] ?? "claude-haiku-4-5-20251001";
        _http.DefaultRequestHeaders.Add("x-api-key",         config["AI:Claude:ApiKey"]);
        _http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        _http.Timeout = TimeSpan.FromSeconds(60);
    }

    public async Task<LLMResponse> ChatWithToolsAsync(
        List<MessageDto>     messages,
        List<ToolDefinition> tools,
        string               systemPrompt)
    {
        var payload = new
        {
            model      = _model,
            max_tokens = 4096,
            system     = systemPrompt,
            tools      = tools.Select(t => new
            {
                name         = t.Name,
                description  = t.Description,
                input_schema = t.Parameters
            }),
            messages = messages
                .Where(m => m.Role != "tool")
                .Select(m => new { role = m.Role, content = m.Content ?? "" })
        };

        var response = await _http.PostAsJsonAsync(
            "https://api.anthropic.com/v1/messages", payload);
        response.EnsureSuccessStatusCode();

        var raw = await response.Content.ReadFromJsonAsync<ClaudeRawResponse>();
        return MapResponse(raw!);
    }

    private static LLMResponse MapResponse(ClaudeRawResponse raw)
    {
        var textBlock = raw.Content.FirstOrDefault(c => c.Type == "text")?.Text ?? "";
        var toolUse   = raw.Content.Where(c => c.Type == "tool_use").ToList();

        if (!toolUse.Any())
            return new LLMResponse { Content = textBlock, FinishReason = "stop" };

        return new LLMResponse
        {
            Content      = textBlock,
            FinishReason = "tool_calls",
            ToolCalls    = toolUse.Select(tc =>
            {
                var argsRaw = JsonSerializer.Serialize(tc.Input);
                return new ToolCall
                {
                    Id           = tc.Id ?? Guid.NewGuid().ToString(),
                    Name         = tc.Name ?? "",
                    ArgumentsRaw = argsRaw,
                    Arguments    = JsonSerializer.Deserialize<JsonElement>(argsRaw)
                };
            }).ToList()
        };
    }

    // ─── Modèles privés (nested) ─────────────────────────────────
    private class ClaudeRawResponse
    {
        [JsonPropertyName("content")]     public List<ClaudeBlock> Content    { get; set; } = new();
        [JsonPropertyName("stop_reason")] public string            StopReason { get; set; } = "";
    }

    private class ClaudeBlock
    {
        [JsonPropertyName("type")]  public string       Type  { get; set; } = "";
        [JsonPropertyName("text")]  public string?      Text  { get; set; }
        [JsonPropertyName("id")]    public string?      Id    { get; set; }
        [JsonPropertyName("name")]  public string?      Name  { get; set; }
        [JsonPropertyName("input")] public JsonElement? Input { get; set; }
    }
}