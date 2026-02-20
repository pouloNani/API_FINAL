using System.Text.Json;

namespace Core.DTOs.Agent;

// ─── Requêtes entrantes ──────────────────────────────────────

public class AgentChatRequest
{
    public string  Message   { get; set; } = "";
    public double? Latitude  { get; set; }
    public double? Longitude { get; set; }
    public string? CartId    { get; set; }
}

public class ConfirmActionRequest
{
    public string        ActionType { get; set; } = "";
    public ActionPayload Payload    { get; set; } = new();
    public string?       CartId     { get; set; }
}

public class ActionPayload
{
    public int ProductId { get; set; }
    public int ShopId    { get; set; }
    public int Quantity  { get; set; } = 1;
}

// ─── Réponses sortantes ──────────────────────────────────────

public class AgentResponse
{
    public string               Message         { get; set; } = "";
    public List<ProposedAction> ProposedActions { get; set; } = new();
    public ConversationState    State           { get; set; } = ConversationState.Idle;
    public string?              ProviderUsed    { get; set; }
}

public class ProposedAction
{
    public string Id          { get; set; } = Guid.NewGuid().ToString();
    public string Label       { get; set; } = "";
    public string Description { get; set; } = "";
    public string ActionType  { get; set; } = "";
    public int    ProductId   { get; set; }
    public int    ShopId      { get; set; }
    public int    Quantity    { get; set; } = 1;
}

public enum ConversationState
{
    Idle,
    AwaitingConfirmation,
    ActionExecuted
}

// ─── LLM Modèles internes ────────────────────────────────────

public class LLMResponse
{
    public string          Content      { get; set; } = "";
    public List<ToolCall>? ToolCalls    { get; set; }
    public string          FinishReason { get; set; } = "";
}

public class ToolCall
{
    public string      Id           { get; set; } = "";
    public string      Name         { get; set; } = "";
    public string      ArgumentsRaw { get; set; } = "";
    public JsonElement Arguments    { get; set; }
}

public class MessageDto
{
    public string          Role       { get; set; } = "";
    public string?         Content    { get; set; }
    public string?         ToolCallId { get; set; }
    public string?         ToolName   { get; set; }
    public List<ToolCall>? ToolCalls  { get; set; }
}

public class ToolDefinition
{
    public string Name        { get; set; } = "";
    public string Description { get; set; } = "";
    public object Parameters  { get; set; } = new();
}