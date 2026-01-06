using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace OmniChat.Application.Hubs;

[Authorize] // Requer token JWT do atendente logado
public class ChatHub : Hub
{
    // Quando o atendente conecta no Dashboard
    public override async Task OnConnectedAsync()
    {
        // Extrai o OrgId do Claim do usuário (JWT)
        var orgId = Context.User.FindFirst("OrganizationId")?.Value;
        
        if (!string.IsNullOrEmpty(orgId))
        {
            // Adiciona o atendente ao grupo da sua empresa
            await Groups.AddToGroupAsync(Context.ConnectionId, orgId);
        }
        
        await base.OnConnectedAsync();
    }

    // Método para o atendente enviar mensagem (Chat Interno ou Resposta ao Cliente)
    public async Task SendMessageToCustomer(string customerUserId, string message)
    {
        // Lógica para enviar mensagem manual via WhatsAppChannel
        // ...
    }
}