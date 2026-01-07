using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using OmniChat.Domain.Entities;
using OmniChat.Domain.Flows;
using OmniChat.Domain.MCP;

namespace OmniChat.Infrastructure.Persistence;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public IMongoClient Client { get; }

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoConnection");
        var databaseName = configuration["MongoSettings:DatabaseName"];
        
        var client = new MongoClient(connectionString);
        
        // --- 2. ATRIBUA O CLIENT À PROPRIEDADE PÚBLICA ---
        Client = client;
        
        _database = client.GetDatabase(databaseName);
    }

    // Acesso tipado às coleções
    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<Plan> Plans => _database.GetCollection<Plan>("plans");
    public IMongoCollection<ConversationFlow> Flows => _database.GetCollection<ConversationFlow>("flows");
    public IMongoCollection<Organization> Organizations => _database.GetCollection<Organization>("organizations");
    
    // Se formos salvar o histórico MCP no Mongo também:
    public IMongoCollection<McpContextSession> McpSessions => _database.GetCollection<McpContextSession>("mcp_sessions");
}