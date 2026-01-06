using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OmniChat.Domain.Entities;

public class Plan
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    public string Name { get; set; } // "Básico", "Profissional", "Customizado"
    public decimal Price { get; set; } // 89.90, 159.90
    public string Currency { get; set; } = "BRL";

    // --- Limites Quantitativos (Hard Limits) ---
    public int MaxUsers { get; set; } // 2, 5, ou -1 (Ilimitado/Sob Demanda)
    public int TrialDays { get; set; } // 5 dias
    
    // --- Controle de Features (Booleans baseados na imagem) ---
    public PlanFeatures Features { get; set; }

    public bool IsCustom { get; set; } // Para o plano "Fale Conosco"
    public bool IsActive { get; set; }
}