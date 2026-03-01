using Microsoft.Extensions.DependencyInjection;
using pz_server_administrator.Models;

namespace pz_server_administrator.Services.Ai;

public class AiProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public AiProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IAiProvider GetProvider(AiProviderType type)
    {
        return type switch
        {
            AiProviderType.Gemini => _serviceProvider.GetRequiredService<GeminiProvider>(),
            AiProviderType.OpenAI => _serviceProvider.GetRequiredService<OpenAiProvider>(),
            AiProviderType.Anthropic => _serviceProvider.GetRequiredService<AnthropicProvider>(),
            AiProviderType.Ollama => _serviceProvider.GetRequiredService<OllamaProvider>(),
            _ => throw new NotSupportedException($"El proveedor de IA '{type}' no está implementado o no es compatible.")
        };
    }
}
