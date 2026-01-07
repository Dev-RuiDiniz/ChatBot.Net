using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using OmniChat.Domain.Interfaces;
using OmniChat.Domain.MCP;

public class OpenAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public string ProviderName => "ChatGPT";

    public OpenAIService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["AI:OpenAI:ApiKey"];
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
    }
    
    public async Task<string> GetResponseAsync(List<McpMessage> history, AiConfig config)
    {
        var messages = new List<object>();

        // 1. Injeta a Persona do Cliente como System Message
        messages.Add(new { role = "system", content = config.BasePersona });

        // 2. Adiciona o histórico do MCP
        messages.AddRange(history.Select(h => new { role = h.Role.ToString().ToLower(), content = h.Content }));

        var payload = new
        {
            model = "gpt-4-turbo",
            messages = messages,
            temperature = config.Temperature
        };

        // ... envio HTTP ...
    }

    public async Task<string> GetResponseAsync(string userMessage, string contextId)
    {
        var payload = new
        {
            model = "gpt-4-turbo",
            messages = new[] 
            {
                new { role = "system", content = "Você é um assistente útil." },
                new { role = "user", content = userMessage }
            }
        };

        var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", payload);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        return result.choices[0].message.content;
    }
}