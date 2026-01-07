using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OmniChat.Domain.Entities;

public class KnowledgeDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }

    public string Title { get; set; }
    public string ContentChunk { get; set; } // O trecho do texto
    
    // O vetor de embeddings (ex: 1536 dimensões para OpenAI Ada-002)
    public double[] Embedding { get; set; } 
    
    public bool IsActive { get; set; }
}