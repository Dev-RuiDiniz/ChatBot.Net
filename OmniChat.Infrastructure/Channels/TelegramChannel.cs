using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using OmniChat.Domain.Interfaces;

namespace OmniChat.Infrastructure.Channels;

public class TelegramChannel : IMessagingChannel
{
    private readonly HttpClient _httpClient;
    private readonly string _botToken;

    public string ChannelName => "Telegram";

    public TelegramChannel(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _botToken = config["Telegram:BotToken"];
    }

    public async Task SendMessageAsync(string recipientId, string plainTextMessage)
    {
        var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
        var payload = new { chat_id = recipientId, text = plainTextMessage };

        var response = await _httpClient.PostAsJsonAsync(url, payload);
        response.EnsureSuccessStatusCode();
    }
}