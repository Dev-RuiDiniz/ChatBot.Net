namespace OmniChat.Domain.Entities;

public class Plan
{
    public int Id { get; set; }
    public string Name { get; set; } // "Free", "Pro", "Enterprise"
    public int MonthlyMessageLimit { get; set; }
    public bool CanUseGPT4 { get; set; } // Se false, usa GPT-3.5 ou Gemini Flash
    public bool CanUseGeminiPro { get; set; }
    public int ContextWindowSize { get; set; } // Limite de tokens para memória
}