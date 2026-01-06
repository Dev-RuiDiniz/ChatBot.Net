using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OmniChat.Domain.Entities;

public class UserSubscription
{
    [BsonRepresentation(BsonType.String)]
    public Guid PlanId { get; set; } // Referência ao documento na coleção 'plans'
    
    [BsonIgnore]
    public Plan? CachedPlanDetails { get; set; } // Preenchido em runtime se necessário

    public DateTime StartDate { get; set; }
    public DateTime? NextBillingDate { get; set; }
    
    // Controle de uso em tempo real
    public int MessagesUsedThisMonth { get; set; }
    public DateTime LastUsageReset { get; set; }

    public bool IsValid() 
    {
        // Lógica simples de validação de data
        return NextBillingDate == null || NextBillingDate > DateTime.UtcNow;
    }
}