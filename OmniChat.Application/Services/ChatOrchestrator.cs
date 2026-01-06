using OmniChat.Domain.Interfaces;

namespace OmniChat.Application.Services;

public class ChatOrchestrator
{
    private readonly IEnumerable<IAIService> _aiServices;
    private readonly IEnumerable<IMessagingChannel> _channels;
    
    // Injeção de dependência de estratégia
    public ChatOrchestrator(IEnumerable<IAIService> aiServices, IEnumerable<IMessagingChannel> channels)
    {
        _aiServices = aiServices;
        _channels = channels;
    }

    public async Task HandleIncomingMessage(string channelName, string userId, string userMessage)
    {
        // 1. Identificar o canal (Whats, Telegram, etc)
        var channel = _channels.FirstOrDefault(c => c.ChannelName == channelName);
        if (channel == null) throw new Exception("Canal não suportado");

        // 2. Escolher a IA (Pode ser via configuração dinâmica ou Round Robin)
        // Exemplo: Usar Gemini para textos longos, GPT para precisão
        var aiService = _aiServices.FirstOrDefault(a => a.ProviderName == "ChatGPT"); 

        // 3. Obter resposta da IA
        string aiResponse = await aiService.GetResponseAsync(userMessage, userId);

        // 4. Enviar resposta de volta ao usuário
        await channel.SendMessageAsync(userId, aiResponse);
        
        // TODO: Persistir logs no banco de dados aqui
    }
}