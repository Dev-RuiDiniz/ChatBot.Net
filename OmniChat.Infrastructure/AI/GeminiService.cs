using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using OmniChat.Domain.Interfaces;

namespace OmniChat.Infrastructure.AI;

public class GeminiService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    public string ProviderName => "Gemini";

    public GeminiService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["AI:Gemini:ApiKey"];
    }

    public async Task<string> GetResponseAsync(string userMessage, string contextId)
    {
        // Implementação específica da API do Google Vertex AI / Gemini
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_apiKey}";
        var payload = new 
        {
            contents = new[] { new { parts = new[] { new { text = userMessage } } } }
        };

        var response = await _httpClient.PostAsJsonAsync(url, payload);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        return result.candidates[0].content.parts[0].text;
    }
}