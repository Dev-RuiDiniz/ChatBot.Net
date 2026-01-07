using MongoDB.Bson.Serialization.Attributes;

namespace OmniChat.Domain.Entities;

public class ProcessedWebhookId
{
    [BsonId]
    public string MessageId { get; set; }
    
    public DateTime ProcessedAt { get; set; }
}