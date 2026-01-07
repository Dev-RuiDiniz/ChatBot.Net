using Microsoft.AspNetCore.Mvc;
using OmniChat.Application.Services;
using OmniChat.Domain.Interfaces;
using OmniChat.Domain.ValueObjects;
using OmniChat.Infrastructure.Persistence; // Para IdempotencyService
using OmniChat.Infrastructure.Repositories;
using OmniChat.Shared.DTOs; // Importante para WhatsAppPayload
using System.Text.Json;

namespace OmniChat.API.Controllers;

[ApiController]
[Route("api/webhook")]
public class WebhookController : ControllerBase
{
    // Usamos o SecureChatOrchestrator (V2) que tem o método ProcessMessageAsync
    private readonly SecureChatOrchestrator _orchestrator;
    private readonly IConfiguration _config;
    
    // Serviços que estavam faltando na injeção de dependência
    private readonly IdempotencyService _idempotencyService;
    private readonly UserRepository _userRepo;
    private readonly ILogger<WebhookController> _logger;
    private readonly IMessagingChannel _whatsAppChannel; // Ou IEnumerable<IMessagingChannel>

    public WebhookController(
        SecureChatOrchestrator orchestrator, 
        IConfiguration config,
        IdempotencyService idempotencyService,
        UserRepository userRepo,
        ILogger<WebhookController> logger,
        IEnumerable<IMessagingChannel> channels)
    {
        _orchestrator = orchestrator;
        _config = config;
        _idempotencyService = idempotencyService;
        _userRepo = userRepo;
        _logger = logger;
        
        // Seleciona o canal específico para envio de resposta
        _whatsAppChannel = channels.FirstOrDefault(c => c.ChannelName == "WhatsApp") 
                           ?? throw new Exception("Canal WhatsApp não configurado.");
    }

    // --- VERIFICAÇÃO DO TOKEN (GET) ---
    [HttpGet("meta")]
    public IActionResult VerifyMetaToken([FromQuery(Name = "hub.mode")] string mode,
                                         [FromQuery(Name = "hub.verify_token")] string token,
                                         [FromQuery(Name = "hub.challenge")] string challenge)
    {
        if (mode == "subscribe" && token == _config["Meta:VerifyToken"])
        {
            return Ok(challenge);
        }
        return Unauthorized();
    }

    // --- RECEBIMENTO DE MENSAGEM (POST) ---
    [HttpPost("meta")]
    public async Task<IActionResult> ReceiveMetaMessage([FromBody] JsonElement payload)
    {
        // 1. Extração de ID para Idempotência (Evitar mensagens duplicadas)
        string messageId = ExtractId(payload); 

        if (!string.IsNullOrEmpty(messageId))
        {
            if (await _idempotencyService.IsProcessedAsync(messageId))
            {
                _logger.LogWarning("Mensagem duplicada ignorada: {Id}", messageId);
                return Ok();
            }
        }

        try 
        {
            // Desserializa para o DTO fortemente tipado para facilitar o acesso
            var jsonText = payload.GetRawText();
            var data = JsonSerializer.Deserialize<WhatsAppPayload>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (data?.Entry != null && 
                data.Entry.Count > 0 && 
                data.Entry[0].Changes != null && 
                data.Entry[0].Changes.Count > 0 &&
                data.Entry[0].Changes[0].Value.Messages != null &&
                data.Entry[0].Changes[0].Value.Messages.Count > 0)
            {
                var message = data.Entry[0].Changes[0].Value.Messages[0];
                string from = message.From; // Telefone
                string text = message.Text?.Body;

                if (!string.IsNullOrEmpty(text))
                {
                    // Lógica principal: 
                    // 1. Achar usuário pelo telefone
                    // 2. Processar mensagem (IA/Fluxo)
                    // 3. Enviar resposta
                    await ProcessWhatsAppFlow(from, text);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro parsing Meta Payload");
        }

        return Ok();
    }

    // Método auxiliar privado para processar a lógica
    private async Task ProcessWhatsAppFlow(string phoneNumber, string text)
    {
        // 1. Busca ID do usuário pelo telefone
        var user = await _userRepo.GetByPhoneNumberAsync(phoneNumber);
        
        // Se usuário não existe, talvez criar um lead ou ignorar (depende da regra de negócio)
        if (user == null) 
        {
            _logger.LogWarning("Mensagem recebida de número desconhecido: {Phone}", phoneNumber);
            return;
        }

        // 2. Orquestrador processa e retorna texto criptografado
        EncryptedText responseEncrypted = await _orchestrator.ProcessMessageAsync(user.Id, text);

        // 3. Descriptografa APENAS para enviar à API do WhatsApp
        string plainResponse = responseEncrypted.ToPlainText(_config["Security:MasterEncryptionKey"]);
    
        // 4. Envia resposta via Canal
        await _whatsAppChannel.SendMessageAsync(user.PhoneNumber, plainResponse);
    }

    // Método auxiliar para extrair o ID do JSON bruto
    private string ExtractId(JsonElement payload)
    {
        try
        {
            // Navegação segura no JSON dinâmico
            if (payload.TryGetProperty("entry", out var entry) && entry.GetArrayLength() > 0)
            {
                var changes = entry[0].GetProperty("changes");
                if (changes.GetArrayLength() > 0)
                {
                    var value = changes[0].GetProperty("value");
                    if (value.TryGetProperty("messages", out var messages) && messages.GetArrayLength() > 0)
                    {
                        return messages[0].GetProperty("id").GetString();
                    }
                }
            }
        }
        catch 
        { 
            // Ignora falhas de estrutura, retorna null
        }
        return null;
    }
}