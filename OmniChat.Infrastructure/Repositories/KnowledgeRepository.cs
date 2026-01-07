using MongoDB.Bson;
using MongoDB.Driver;
using OmniChat.Domain.Entities;
using OmniChat.Infrastructure.Persistence;

namespace OmniChat.Infrastructure.Repositories;

public class KnowledgeRepository
{
    private readonly IMongoCollection<KnowledgeDocument> _collection;

    public KnowledgeRepository(MongoDbContext db)
    {
        _collection = db.Database.GetCollection<KnowledgeDocument>("knowledge_base");
    }

    public async Task<List<string>> SearchSimilarContextAsync(Guid orgId, double[] queryVector, int limit = 3)
    {
        // Pipeline de Agregação para Vector Search (Requer índice no Atlas)
        var pipeline = new BsonDocument[]
        {
            new BsonDocument("$vectorSearch", new BsonDocument
            {
                { "index", "vector_index" },
                { "path", "Embedding" },
                { "queryVector", new BsonArray(queryVector) },
                { "numCandidates", 100 },
                { "limit", limit },
                { "filter", new BsonDocument("OrganizationId", orgId.ToString()) } // Filtro de segurança Multi-Tenant
            }),
            new BsonDocument("$project", new BsonDocument
            {
                { "ContentChunk", 1 },
                { "score", new BsonDocument("$meta", "vectorSearchScore") }
            })
        };

        var results = await _collection.Aggregate<BsonDocument>(pipeline).ToListAsync();
        
        // Retorna apenas os textos encontrados
        return results.Select(x => x["ContentChunk"].AsString).ToList();
    }
}