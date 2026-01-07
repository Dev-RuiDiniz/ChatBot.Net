using OmniChat.Domain.Enums;
using OmniChat.Domain.ValueObjects;

namespace OmniChat.Domain.MCP;

public class McpMessage
{
    public Role Role { get; set; }
    public EncryptedText Content { get; set; } // O conteúdo nunca fica em PlainText na Heap por muito tempo
    public DateTime Timestamp { get; set; }
    public string? ProviderMetadata { get; set; } // "OpenAI-GPT4" ou "Google-Gemini"

    public McpMessage(Role role, EncryptedText content, DateTime timestamp, string? provider = null)
    {
        Role = role;
        Content = content;
        Timestamp = timestamp;
        ProviderMetadata = provider;
    }
}