using Polly;
using Polly.Extensions.Http;

namespace OmniChat.Infrastructure.Resilience;

public static class ResiliencePolicies
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // Trata 5xx, 408 e falhas de rede
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // Trata Rate Limits (429)
            .WaitAndRetryAsync(3, retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))); // Backoff Exponencial: 2s, 4s, 8s
    }
}