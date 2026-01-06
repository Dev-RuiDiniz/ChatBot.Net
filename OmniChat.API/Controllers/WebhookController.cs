using Microsoft.AspNetCore.Mvc;
using OmniChat.Application.Services;
using OmniChat.Domain.ValueObjects;

namespace OmniChat.API.Controllers;

[ApiController]
[Route("api/webhook")]
public class WebhookController : ControllerBase
{
    private readonly ChatOrchestrator _orchestrator;
    private readonly IConfiguration _config;

    public WebhookController(ChatOrchestrator orchestrator, IConfiguration config)
    {
        _orchestrator = orchestrator;
        _config = config;
    }

    // --- WHATSAPP & INSTAGRAM (Meta utiliza o mesmo padrão) ---
    
    [HttpGet("meta")]
    public IActionResult VerifyMetaToken([FromQuery(Name = "hub.mode")] string mode,
                                         [FromQuery(Name = "hub.verify_token")] string token,
                                         [FromQuery(Name = "hub.challenge")] string challenge)
    {
        // Validação necessária para ativar o webhook no painel do Facebook Developers
        if (mode == "subscribe" && token == _config["Meta:VerifyToken"])
        {
            return Ok(challenge);
        }
        return Unauthorized();
    }

    [HttpPost("meta")]
    public async Task<IActionResult> ReceiveMetaMessage([FromBody] dynamic payload)
    {
        // Lógica simplificada de extração. Na prática, crie DTOs fortes.
        // O JSON do WhatsApp Business API é aninhado.
        try 
        {
            var entry = payload.GetProperty("entry")[0];
            var changes = entry.GetProperty("changes")[0];
            var value = changes.GetProperty("value");
            
            if (value.TryGetProperty("messages", out var messages))
            {
                var message = messages[0];
                string from = message.GetProperty("from").GetString();
                string text = message.GetProperty("text").GetProperty("body").GetString();

                // Dispara o processamento em background para não bloquear o webhook
                _ = _orchestrator.HandleIncomingMessage("WhatsApp", from, text);
            }
        }
        catch (Exception ex)
        {
            // Logar erro, mas retornar 200 OK para o Facebook não tentar reenviar infinitamente
            Console.WriteLine($"Erro parsing Meta: {ex.Message}");
        }

        return Ok();
    }

    // --- TELEGRAM ---

    [HttpPost("telegram")]
    public async Task<IActionResult> ReceiveTelegramMessage([FromBody] dynamic update)
    {
        try
        {
            var message = update.GetProperty("message");
            string chatId = message.GetProperty("chat").GetProperty("id").ToString();
            string text = message.GetProperty("text").GetString();

            _ = _orchestrator.HandleIncomingMessage("Telegram", chatId, text);
        }
        catch
        {
            // Log error
        }
        return Ok();
    }
    
    [HttpPost("webhook/whatsapp")]
    public async Task<IActionResult> WhatsAppWebhook([FromBody] WhatsAppPayload payload)
    {
        // Extração do ID do usuário (Número de telefone)
        // Na prática, deve-se mapear Telefone -> Guid UserId no banco
        Guid userId = await _userRepo.GetUserIdByPhone(payload.Entry[0].Changes[0].Value.Messages[0].From);
        string text = payload.Entry[0].Changes[0].Value.Messages[0].Text.Body;

        try 
        {
            // O orquestrador retorna o objeto criptografado
            EncryptedText response = await _orchestrator.ProcessMessageAsync(userId, text);

            // Descriptografar APENAS no momento exato do envio para a API do WhatsApp
            // O "SendToWhatsApp" usará TLS (HTTPS) do transporte padrão
            string plainResponse = response.ToPlainText(_config["Security:MasterEncryptionKey"]);
        
            await _channelService.SendAsync(userId, plainResponse);
        
            return Ok();
        }
        catch (Exception ex)
        {
            // Logar erro (sem logar o conteúdo da mensagem!)
            _logger.LogError(ex, "Erro processando mensagem do usuário {UserId}", userId);
            return Ok(); // Retornar OK para o webhook não tentar reenviar infinito
        }
    }
}