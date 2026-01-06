using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using OmniChat.Application.Hubs;
using OmniChat.Application.Services;
using OmniChat.Domain.Entities;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// --- Configuração Global do Mongo para GUIDs ---
// Isso garante que os GUIDs do C# sejam salvos como strings padrão no Mongo
// Evita problemas de compatibilidade binary entre drivers.
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

// --- Configuração dos Serviços ---
// Singleton é recomendado para MongoClient
builder.Services.AddSingleton<MongoDbContext>();

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<PlanRepository>();
builder.Services.AddScoped<PlanEnforcementService>();

// Configuração via appsettings.json
/*
  "ConnectionStrings": {
    "MongoConnection": "mongodb://admin:senha@localhost:27017"
  },
  "MongoSettings": {
    "DatabaseName": "OmniChatDb"
  }
*/

builder.Services.AddSignalR();

// ... outros serviços ...

var app = builder.Build();

// Mapear rota
app.MapHub<ChatHub>("/chathub");

// --- Seed Inicial (Opcional) ---
// Criar um plano padrão se não existir
using (var scope = app.Services.CreateScope())
{
    var planRepo = scope.ServiceProvider.GetRequiredService<PlanRepository>();
    var existingPlans = await planRepo.GetAllActivePlansAsync();
    
    if (!existingPlans.Any())
    {
        var freePlan = new Plan 
        { 
            Id = Guid.NewGuid(), 
            Name = "Free Tier", 
            MonthlyMessageLimit = 50, 
            AllowGpt4 = false 
        };
        await planRepo.CreatePlanAsync(freePlan);
    }
}

app.Run();