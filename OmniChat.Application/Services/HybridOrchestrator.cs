namespace OmniChat.Application.Services;

public class HybridOrchestrator
{
    private readonly McpService _mcpService;
    private readonly FlowEngineService _flowEngine;
    private readonly SecureChatOrchestrator _aiOrchestrator;
    private readonly IHubContext<ChatHub> _hubContext; // SignalR

    public async Task ProcessIncomingMessage(Guid userId, Guid orgId, string userMessage)
    {
        var session = await _mcpService.LoadSessionAsync(userId);

        // 1. Verificação: Atendimento Humano Ativo?
        if (session.IsHandedOverToHuman)
        {
            // Apenas salva no histórico e notifica o painel do atendente via SignalR
            await NotifyDashboard(orgId, userId, userMessage, "User");
            return; 
        }

        // 2. Verificação: Está em um Fluxo?
        if (session.IsInFlow)
        {
            var flowResponse = await _flowEngine.ProcessStepAsync(session, userMessage);
            
            if (flowResponse.HasResponse)
            {
                await SendToChannel(userId, flowResponse.Message);
                await NotifyDashboard(orgId, userId, flowResponse.Message, "Bot-Flow");
                return;
            }
            // Se o fluxo terminou ou devolveu para IA, continue...
        }

        // 3. Fallback para IA (Se o plano permitir)
        // Aqui chamamos o orquestrador de IA que criamos anteriormente
        var aiResponse = await _aiOrchestrator.ProcessMessageAsync(userId, userMessage);
        
        await SendToChannel(userId, aiResponse.ToPlainText());
        await NotifyDashboard(orgId, userId, aiResponse.ToPlainText(), "Bot-AI");
    }

    private async Task NotifyDashboard(Guid orgId, Guid userId, string message, string senderType)
    {
        // Envia para o grupo da Organização via SignalR
        await _hubContext.Clients.Group(orgId.ToString())
            .SendAsync("ReceiveMessage", new { UserId = userId, Message = message, Type = senderType });
    }
}