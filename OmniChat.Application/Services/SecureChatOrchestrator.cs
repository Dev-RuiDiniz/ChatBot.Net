using Microsoft.Extensions.Configuration;
using OmniChat.Domain.Interfaces;
using OmniChat.Domain.ValueObjects;

namespace OmniChat.Application.Services;

public class SecureChatOrchestrator
{
    private readonly McpService _mcpService;
    private readonly IPlanEnforcementService _planService;
    private readonly IAIFactory _aiFactory; // Factory para escolher GPT ou Gemini
    private readonly string _masterKey;

    public SecureChatOrchestrator(
        McpService mcpService, 
        IPlanEnforcementService planService, 
        IAIFactory aiFactory,
        IConfiguration config)
    {
        _mcpService = mcpService;
        _planService = planService;
        _aiFactory = aiFactory;
        _masterKey = config["Security:MasterEncryptionKey"];
    }

    public async Task<EncryptedText> ProcessMessageAsync(Guid userId, string rawUserMessage)
    {
        // 1. Verificação de Plano (Bloqueia antes de processar)
        var subscription = await _planService.GetSubscriptionAsync(userId);
        if (!subscription.CanSendMessage())
            throw new Exception("Limite do plano excedido.");

        // 2. Carregar Contexto MCP (Fonte da verdade)
        var session = await _mcpService.LoadSessionAsync(userId);

        // 3. Criptografar entrada IMEDIATAMENTE e adicionar ao MCP
        // Isso garante que se o sistema cair agora, a mensagem do usuário já está salva no histórico.
        var encryptedInput = EncryptedText.FromPlainText(rawUserMessage, _masterKey);
        session.AddUserInteraction(encryptedInput);
        await _mcpService.CommitStateAsync(session);

        // 4. Seleção de Provedor de IA baseado no Plano
        // Ex: Usuário Enterprise usa GPT-4, Free usa Gemini-Flash
        IAIService aiProvider = _aiFactory.GetProvider(subscription.Plan);

        // 5. Preparar Contexto para IA (Descriptografia Temporária)
        // Convertemos o histórico MCP para o formato que a IA entende
        var contextHistory = session.GetDecryptedHistory(_masterKey);

        // 6. Chamada à IA
        string rawAiResponse;
        try 
        {
            rawAiResponse = await aiProvider.GenerateResponseAsync(contextHistory);
        }
        catch (Exception ex)
        {
            // Fallback strategy: Se OpenAI cair, tenta Gemini automaticamente
            // Isso garante continuidade sem erro para o usuário
            var fallbackProvider = _aiFactory.GetFallbackProvider(subscription.Plan);
            rawAiResponse = await fallbackProvider.GenerateResponseAsync(contextHistory);
        }

        // 7. Criptografar resposta IMEDIATAMENTE
        var encryptedOutput = EncryptedText.FromPlainText(rawAiResponse, _masterKey);

        // 8. Atualizar MCP e Persistir
        session.AddAiResponse(encryptedOutput, aiProvider.ProviderName);
        
        // Decrementa quota do plano
        await _planService.RegisterUsageAsync(userId);
        await _mcpService.CommitStateAsync(session);

        return encryptedOutput;
    }
}