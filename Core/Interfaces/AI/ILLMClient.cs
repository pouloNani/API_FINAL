using Core.DTOs.Agent;

namespace Core.Interfaces;

public interface ILLMClient
{
    string ProviderName { get; }

    Task<LLMResponse> ChatWithToolsAsync(
        List<MessageDto>     messages,
        List<ToolDefinition> tools,
        string               systemPrompt);
}