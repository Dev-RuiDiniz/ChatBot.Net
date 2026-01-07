using MongoDB.Driver;
using OmniChat.Domain.Entities;

namespace OmniChat.Infrastructure.Persistence;

public class IdempotencyService
{
    private readonly IMongoCollection<ProcessedWebhookId> _collection;

    public IdempotencyService(MongoDbContext context)
    {
        _collection = context.Database.GetCollection<ProcessedWebhookId>("processed_webhooks");
        
        // Criar índice TTL para apagar IDs após 24h automaticamente
        var indexKeys = Builders<ProcessedWebhookId>.IndexKeys.Ascending(x => x.ProcessedAt);
        var indexOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.FromHours(24) };
        var model = new CreateIndexModel<ProcessedWebhookId>(indexKeys, indexOptions);
        _collection.Indexes.CreateOne(model);
    }

    public async Task<bool> IsProcessedAsync(string messageId)
    {
        // Tenta inserir. Se falhar por chave duplicada, já foi processado.
        try
        {
            await _collection.InsertOneAsync(new ProcessedWebhookId 
            { 
                MessageId = messageId, 
                ProcessedAt = DateTime.UtcNow 
            });
            return false; // Não foi processado antes (Sucesso na inserção)
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return true; // Já existe, ignorar
        }
    }
}