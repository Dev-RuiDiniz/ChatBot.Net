namespace OmniChat.Domain.Interfaces
{
    public interface IAIService
    {
        // Define qual provedor (GPT ou Gemini)
        string ProviderName { get; }
        Task<string> GenerateResponseAsync(List<(string Role, string Content)> history);
        Task<string> GetResponseAsync(string userMessage, string contextId);
    }
}