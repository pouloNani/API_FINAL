using System.Linq;
using System.Text.Json;
using Core.DTOs.Agent;
using Core.Interfaces;

namespace Infrastructure.AI;

public class AgentLoop
{
    private readonly ILLMClient          _llm;
    private readonly ToolExecutor        _executor;
    private readonly ConversationHistory _history;
    private const int MaxTurns          = 6;
    private const int MaxHistoryMessages = 20;

    public AgentLoop(ILLMClient llm, ToolExecutor executor, ConversationHistory history)
    {
        _llm      = llm;
        _executor = executor;
        _history  = history;
    }

    private const string SystemPrompt = """
    Tu es un assistant shopping intelligent pour une plateforme de commerce local.
    
    ═══ RÈGLES DE RECHERCHE ═══
    - Client cherche un produit → search_products avec query = mot-clé (ex: "casque", "pain", "bière")
    - Client veut voir tous les shops → search_shops sans paramètres
    - Client veut les produits d'un shop → get_products_by_shop avec le shop_id issu de search_shops
    - Client veut les promos → search_best_promotions ou get_promotions_by_shop
    - TOUJOURS appeler propose_actions après une recherche
    - Ne JAMAIS passer des strings vides "" comme paramètres, omets-les si pas de valeur
    - Ne JAMAIS inventer des shop_id ou product_id — uniquement ceux venant des résultats tools

    ═══ RÈGLES PANIER ═══
    - Proposer "add_to_cart" avec productId + shopId + quantity réels
    - Ne jamais ajouter au panier sans confirmation explicite du client
    
    ═══ RÈGLES AUTH ═══
    - Inscription/Connexion → demander email D'ABORD, puis mot de passe dans un 2ème message
    - Après login/register réussi → continuer la conversation normalement
    
    ═══ RÈGLES propose_actions ═══
    action_type UNIQUEMENT parmi : "add_to_cart", "view_shop", "get_details", "navigate"
    - "add_to_cart" → nécessite productId + shopId + quantity
    - "view_shop" → nécessite shopId
    - "get_details" → nécessite productId
    - "navigate" → navigation générale
    
    ═══ RÈGLES GÉNÉRALES ═══
    - Répondre en français, naturellement et de façon concise
    - Ne jamais générer un JSON incomplet ou malformé
    - En cas de doute → appeler un tool plutôt que répondre en texte libre
    - Retenir le contexte de la conversation
    - Ne jamais appeler propose_actions si la recherche n'a retourné aucun résultat
    - Si aucun produit trouvé → répondre en texte simple et demander au client de reformuler
    """;

    public async Task<AgentResponse> RunAsync(AgentChatRequest request, string userId)
    {
        var history = await _history.GetAsync(userId);
        _executor.ResetActions();

        var userMsg = new MessageDto
        {
            Role    = "user",
            Content = BuildContextualMessage(request, userId)
        };

        var messages = history.TakeLast(MaxHistoryMessages).ToList();
        messages.Add(userMsg);

        for (int turn = 0; turn < MaxTurns; turn++)
        {
            LLMResponse response;
            try
            {
                response = await _llm.ChatWithToolsAsync(messages, ToolDefinitions.All, SystemPrompt);
                response = await _llm.ChatWithToolsAsync(messages, ToolDefinitions.All, SystemPrompt);

                // DEBUG TEMPORAIRE
                Console.WriteLine($"[Turn {turn}] FinishReason: {response.FinishReason}");
                Console.WriteLine($"[Turn {turn}] Content: {response.Content}");
                Console.WriteLine($"[Turn {turn}] ToolCalls: {response.ToolCalls?.Count ?? 0}");
            }
            catch (Exception ex)
            {
                await PersistExchangeAsync(userId, userMsg, "Erreur lors de la communication avec le LLM.");
                return new AgentResponse { Message = "Une erreur est survenue, veuillez réessayer." };
            }

            if (response.ToolCalls == null || !response.ToolCalls.Any())
            {
                await PersistExchangeAsync(userId, userMsg, response.Content);

                return new AgentResponse
                {
                    Message         = response.Content,
                    ProposedActions = _executor.CapturedActions,
                    State           = _executor.CapturedActions.Any()
                                        ? ConversationState.AwaitingConfirmation
                                        : ConversationState.Idle,
                    ProviderUsed    = _llm.ProviderName
                };
            }

            messages.Add(new MessageDto
            {
                Role      = "assistant",
                Content   = response.Content,
                ToolCalls = response.ToolCalls
            });

            foreach (var toolCall in response.ToolCalls)
            {
                string result;
                try
                {
                    result = await _executor.ExecuteAsync(toolCall.Name, toolCall.Arguments);
                }
                catch (Exception ex)
                {
                    result = JsonSerializer.Serialize(new { error = ex.Message });
                }

                messages.Add(new MessageDto
                {
                    Role       = "tool",
                    ToolCallId = toolCall.Id,
                    ToolName   = toolCall.Name,
                    Content    = result
                });
            }
        }

        await PersistExchangeAsync(userId, userMsg, "Limite de tours atteinte.");
        return new AgentResponse { Message = "Je n'ai pas compris, pouvez-vous reformuler ?" };
    }

    private async Task PersistExchangeAsync(string userId, MessageDto userMsg, string assistantContent)
    {
        await _history.AppendAsync(userId, userMsg);
        await _history.AppendAsync(userId, new MessageDto
        {
            Role    = "assistant",
            Content = assistantContent
        });
    }

    private static string BuildContextualMessage(AgentChatRequest request, string userId)
    {
        var ctx = new List<string>
        {
            $"UserId:{userId}",
            $"Heure:{DateTime.UtcNow:HH:mm}"
        };

        if (request.Latitude.HasValue && request.Longitude.HasValue)
            ctx.Add($"GPS:{request.Latitude},{request.Longitude}");

        if (!string.IsNullOrEmpty(request.CartId))
            ctx.Add($"CartId:{request.CartId}");

        return $"[{string.Join("|", ctx)}] {request.Message}";
    }
}