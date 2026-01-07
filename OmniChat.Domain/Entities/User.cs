using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using OmniChat.Domain.Enums; // Certifique-se de importar o namespace do Enum criado acima

namespace OmniChat.Domain.Entities;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    
    // --- PROPRIEDADES QUE ESTAVAM FALTANDO ---

    [BsonRepresentation(BsonType.String)]
    public UserRole Role { get; set; } // Agora o AuthController reconhecerá user.Role

    [BsonRepresentation(BsonType.String)]
    public Guid? OrganizationId { get; set; } // Nullable, pois SuperAdmin pode não ter Org

    // -----------------------------------------

    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

    public UserSubscription Subscription { get; set; }

    public User(string phoneNumber)
    {
        Id = Guid.NewGuid();
        PhoneNumber = phoneNumber;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
        Role = UserRole.User; // Define um padrão ao criar
    }
}