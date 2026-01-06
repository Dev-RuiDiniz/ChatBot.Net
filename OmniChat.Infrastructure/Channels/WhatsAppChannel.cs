using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using OmniChat.Domain.Interfaces;

namespace OmniChat.Infrastructure.Channels;

public class WhatsAppChannel : IMessagingChannel
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public string ChannelName => "WhatsApp";

    public WhatsAppChannel(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
        
        // Configura o Token da Meta API (Recomendado: Usar IOptionsSnapshot)
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config["Meta:AccessToken"]);
    }

    public async Task SendMessageAsync(string recipientId, string plainTextMessage)
    {
        // Estrutura oficial da WhatsApp Cloud API
        var payload = new
        {
            messaging_product = "whatsapp",
            to = recipientId,
            type = "text",
            text = new { body = plainTextMessage }
        };

        var phoneNumberId = _config["Meta:PhoneNumberId"];
        var response = await _httpClient.PostAsJsonAsync($"https://graph.facebook.com/v17.0/{phoneNumberId}/messages", payload);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Falha ao enviar para WhatsApp: {error}");
        }
    }
}