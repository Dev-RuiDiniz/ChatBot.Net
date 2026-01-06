using OmniChat.Domain.ValueObjects;

namespace OmniChat.Domain.MCP;

public class McpContextSession
{
    public Guid SessionId { get; private set; }
    public Guid UserId { get; private set; }
    
    // O Histórico é uma lista de mensagens criptografadas
    private readonly List<McpMessage> _history = new();
    
    // Metadados de estado (ex: "Aguardando pagamento", "Coletando endereço")
    public Dictionary<string, string> StateVariables { get; private set; } = new();

    public McpContextSession(Guid userId)
    {
        SessionId = Guid.NewGuid();
        UserId = userId;
    }

    public void AddUserInteraction(EncryptedText message)
    {
        _history.Add(new McpMessage(Role.User, message, DateTime.UtcNow));
    }

    public void AddAiResponse(EncryptedText message, string providerUsed)
    {
        _history.Add(new McpMessage(Role.Assistant, message, DateTime.UtcNow, providerUsed));
    }

    // Retorna histórico descriptografado APENAS para o Service Layer injetar na IA
    public List<(string Role, string Content)> GetDecryptedHistory(string key)
    {
        return _history.Select(h => (h.Role.ToString(), h.Content.ToPlainText(key))).ToList();
    }
}