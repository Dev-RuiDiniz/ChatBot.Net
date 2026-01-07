using Microsoft.Extensions.Configuration;
using OmniChat.Domain.Interfaces;
using OmniChat.Domain.MCP;

namespace OmniChat.Application.Services;

public class McpService
{
    private readonly IMcpRepository _repository;
    private readonly string _masterKey; // Injetado via Secure Configuration (Key Vault)

    public McpService(IMcpRepository repository, IConfiguration config)
    {
        _repository = repository;
        _masterKey = config["Security:MasterEncryptionKey"];
    }

    public async Task<McpContextSession> LoadSessionAsync(Guid userId)
    {
        var session = await _repository.GetSessionByUserIdAsync(userId);
        if (session == null)
        {
            session = new McpContextSession(userId);
            // Salva estado inicial
            await _repository.SaveSessionAsync(session);
        }
        return session;
    }

    public async Task CommitStateAsync(McpContextSession session)
    {
        // Persistência atômica do estado
        await _repository.SaveSessionAsync(session);
    }
}