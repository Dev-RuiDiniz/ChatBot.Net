using OmniChat.Domain.MCP;

namespace OmniChat.Domain.Interfaces;

public interface IMcpRepository
{
    // Busca a sessão ativa do usuário
    Task<McpContextSession?> GetSessionByUserIdAsync(Guid userId);
    
    // Salva ou Atualiza o estado da sessão (Upsert)
    Task SaveSessionAsync(McpContextSession session);
}