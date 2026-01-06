namespace OmniChat.Domain.Interfaces
{
    public interface IMessagingChannel
    {
        string ChannelName { get; } // "WhatsApp", "Telegram", "Instagram"
        Task SendMessageAsync(string recipientId, string message);
    }
}