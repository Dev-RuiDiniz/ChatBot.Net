using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OmniChat.Domain.Entities;

public class UserSubscription
{
    [BsonRepresentation(BsonType.String)]
    public Guid PlanId { get; set; }
    
    // --- CORREÇÃO 1: Renomeado para 'Plan' para compatibilidade com o Orquestrador ---
    [BsonIgnore]
    public Plan? Plan { get; set; } 

    public DateTime StartDate { get; set; }
    public DateTime? NextBillingDate { get; set; }
    
    public int MessagesUsedThisMonth { get; set; }
    public DateTime LastUsageReset { get; set; }

    public bool IsValid() 
    {
        return NextBillingDate == null || NextBillingDate > DateTime.UtcNow;
    }

    // --- CORREÇÃO 2: Implementação do CanSendMessage ---
    public bool CanSendMessage()
    {
        // 1. Segurança: Se o plano não foi carregado pelo Service, bloqueia.
        if (Plan == null) return false;
        
        // 2. Valida vigência da assinatura
        if (!IsValid()) return false;

        // 3. Validação de Limites
        // Como os planos Básico/Pro são "Mensagens Ilimitadas", retornamos true.
        // Se futuramente houver limite numérico, descomente a linha abaixo:
        // if (Plan.Features.MonthlyMessageLimit > 0 && MessagesUsedThisMonth >= Plan.Features.MonthlyMessageLimit) return false;

        return true;
    }
}