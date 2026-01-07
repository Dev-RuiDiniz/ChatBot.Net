using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace OmniChat.Infrastructure.AI;

public class EmbeddingService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public EmbeddingService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["AI:OpenAI:ApiKey"];
    }

    public async Task<double[]> GenerateEmbeddingAsync(string text)
    {
        // Limpa o texto para economizar tokens e melhorar precisão
        text = text.Replace("\n", " ");

        var payload = new
        {
            input = text,
            model = "text-embedding-3-small" // Mais barato e eficiente que o ada-002
        };

        var response = await _http.PostAsJsonAsync("https://api.openai.com/v1/embeddings", payload);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        
        // Conversão do JSON dinâmico para array de double
        var vector = result.data[0].embedding.ToObject<double[]>();
        return vector;
    }
}