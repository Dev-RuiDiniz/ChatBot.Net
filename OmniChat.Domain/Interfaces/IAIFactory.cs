using OmniChat.Domain.Entities;

namespace OmniChat.Domain.Interfaces;

public interface IAIFactory
{
    IAIService GetProvider(Plan plan);
    
    // Método para obter a IA de contingência (fallback)
    IAIService GetFallbackProvider(Plan plan);
}