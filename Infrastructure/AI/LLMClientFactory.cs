using Core.Interfaces;
using Infrastructure.AI.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.AI;

public class LLMClientFactory
{
    private readonly IServiceProvider _services;
    private readonly string           _provider;

    public LLMClientFactory(IServiceProvider services, IConfiguration config)
    {
        _services = services;
        _provider = config["AI:Provider"] ?? "Ollama";
    }

    public ILLMClient Create() => _provider switch
    {
        //"Ollama"  => _services.GetRequiredService<OllamaClient>(),
        "Mistral" => _services.GetRequiredService<MistralClient>(),
        "Gemini"  => _services.GetRequiredService<GeminiClient>(),
        "Claude"  => _services.GetRequiredService<ClaudeAIClient>(),
        "LlamaScout4" => _services.GetRequiredService<ScoutClient>(),
        _         => throw new InvalidOperationException($"Provider IA '{_provider}' non supporté. Valeurs valides: Ollama, Mistral, Gemini, Claude")
    };
}