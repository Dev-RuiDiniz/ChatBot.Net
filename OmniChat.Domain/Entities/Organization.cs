using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OmniChat.Domain.Entities;

public class Organization
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    
    public string Name { get; set; } // Nome da Empresa
    public string CnpjOrTaxId { get; set; }

    // Dados da Assinatura Embarcados
    public OrganizationSubscription Subscription { get; set; }

    // Lista de IDs dos usuários que pertencem a esta empresa
    // No Mongo, é performático manter os IDs aqui para verificação rápida de contagem
    public List<Guid> MemberIds { get; set; } = new();

    public Organization(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
        Subscription = new OrganizationSubscription();
    }
}