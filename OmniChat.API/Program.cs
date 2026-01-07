using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using OmniChat.Application.Hubs;
using OmniChat.Application.Services;
using OmniChat.Application.Validators;
using OmniChat.Domain.Entities;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories;
using FluentValidation.AspNetCore;
using FluentValidation; 

var builder = WebApplication.CreateBuilder(args);

// Configuração do FluentValidation (Agora funcionará com as versões 11.x)
builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

// --- Configuração dos Serviços ---
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<PlanRepository>();
builder.Services.AddScoped<PlanEnforcementService>();

builder.Services.AddSignalR();
builder.Services.AddControllers(); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapHub<ChatHub>("/chathub");
app.MapControllers(); // Não esqueça de mapear os controllers

// --- Seed Inicial ---
using (var scope = app.Services.CreateScope())
{
    var planRepo = scope.ServiceProvider.GetRequiredService<PlanRepository>();
    var existingPlans = await planRepo.GetAllActivePlansAsync();
    
    if (!existingPlans.Any())
    {
        // 2. Correção da estrutura do Plano para bater com a Entidade atualizada
        var freePlan = new Plan 
        { 
            Id = Guid.NewGuid(), 
            Name = "Free Tier",
            Price = 0,
            MaxUsers = 1,
            TrialDays = 0,
            IsActive = true,
            IsCustom = false,
            // As propriedades antigas (AllowGpt4, MonthlyMessageLimit) 
            // foram movidas para dentro de 'Features' na nossa refatoração anterior
            Features = new PlanFeatures 
            {
                HasAiChatbot = false,
                HasInternalChat = true,
                CanSendCampaigns = false,
                MonthlyCampaignMessagesLimit = 0
            }
        };
        await planRepo.CreatePlanAsync(freePlan);
    }
}

app.Run();