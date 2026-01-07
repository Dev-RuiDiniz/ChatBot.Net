using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using OmniChat.Application.Hubs;
using OmniChat.Domain.Interfaces;
using OmniChat.Infrastructure.Repositories;

namespace OmniChat.Application.Services;

public class HybridOrchestrator
{
    private readonly McpService _mcpService;
    private readonly FlowEngineService _flowEngine;
    private readonly SecureChatOrchestrator _aiOrchestrator;
    private readonly IHubContext<ChatHub> _hubContext;
    
    // --- NOVAS DEPENDÊNCIAS NECESSÁRIAS ---
    private readonly IEnumerable<IMessagingChannel> _channels;
    private readonly UserRepository _userRepo;
    private readonly string _masterKey;

    public HybridOrchestrator(
        McpService mcpService,
        FlowEngineService flowEngine,
        SecureChatOrchestrator aiOrchestrator,
        IHubContext<ChatHub> hubContext,
        IEnumerable<IMessagingChannel> channels,
        UserRepository userRepo,
        IConfiguration config)
    {
        _mcpService = mcpService;
        _flowEngine = flowEngine;
        _aiOrchestrator = aiOrchestrator;
        _hubContext = hubContext;
        _channels = channels;
        _userRepo = userRepo;
        _masterKey = config["Security:MasterEncryptionKey"];
    }

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

        // 3. Fallback para IA
        var aiResponse = await _aiOrchestrator.ProcessMessageAsync(userId, userMessage);
        
        // CORREÇÃO: Passando a chave mestra para descriptografar antes de enviar
        string plainResponse = aiResponse.ToPlainText(_masterKey);

        await SendToChannel(userId, plainResponse);
        await NotifyDashboard(orgId, userId, plainResponse, "Bot-AI");
    }

    // --- IMPLEMENTAÇÃO DO MÉTODO QUE FALTAVA ---
    private async Task SendToChannel(Guid userId, string message)
    {
        // Busca o usuário para saber o telefone
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) return;

        // Seleciona o canal (Assumindo WhatsApp por padrão ou lógica de preferência)
        var channel = _channels.FirstOrDefault(c => c.ChannelName == "WhatsApp");
        
        if (channel != null)
        {
            await channel.SendMessageAsync(user.PhoneNumber, message);
        }
    }

    private async Task NotifyDashboard(Guid orgId, Guid userId, string message, string senderType)
    {
        // Envia para o grupo da Organização via SignalR
        await _hubContext.Clients.Group(orgId.ToString())
            .SendAsync("ReceiveMessage", new { UserId = userId, Message = message, Type = senderType });
    }
}