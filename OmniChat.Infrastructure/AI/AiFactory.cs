using OmniChat.Domain.Entities;
using OmniChat.Domain.Interfaces;

namespace OmniChat.Infrastructure.AI;

public class AiFactory : IAIFactory
{
    private readonly IServiceProvider _serviceProvider;

    public IAIService GetProvider(Plan plan)
    {
        // Lógica de decisão baseada no plano
        if (plan.CanUseGPT4) return _serviceProvider.GetRequiredService<OpenAIService>();
        return _serviceProvider.GetRequiredService<GeminiService>();
    }
    
    // ... Implementação do Fallback
}