using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OmniChat.Domain.Flows;

public class ConversationFlow
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    
    public Guid OrganizationId { get; set; } // Pertence a uma empresa
    public string Name { get; set; } // "Fluxo de Triagem Inicial"
    public bool IsActive { get; set; }
    
    // O ponto de entrada (primeira mensagem)
    public string TriggerKeyword { get; set; } // Ex: "oi", "iniciar", ou null (padrão)
    
    // Definição dos Nós (Steps)
    public List<FlowNode> Nodes { get; set; } = new();
}