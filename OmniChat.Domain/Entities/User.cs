using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OmniChat.Domain.Entities;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    public string PhoneNumber { get; set; } // Identificador principal (WhatsApp/Telegram ID)
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

    // O relacionamento com o plano é embarcado aqui
    public UserSubscription Subscription { get; set; }

    public User(string phoneNumber)
    {
        Id = Guid.NewGuid();
        PhoneNumber = phoneNumber;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }
}
