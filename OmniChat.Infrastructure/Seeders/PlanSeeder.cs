using OmniChat.Domain.Entities;
using OmniChat.Infrastructure.Persistence;

namespace OmniChat.Infrastructure.Seeders;

using MongoDB.Driver;

public class PlanSeeder
{
    private readonly MongoDbContext _context;

    public PlanSeeder(MongoDbContext context)
    {
        _context = context;
    }

    public async Task SeedDefaultPlansAsync()
    {
        var count = await _context.Plans.CountDocumentsAsync(_ => true);
        if (count > 0) return;

        var plans = new List<Plan>
        {
            // --- PLANO BÁSICO ---
            new Plan
            {
                Id = Guid.NewGuid(),
                Name = "Básico",
                Price = 89.90m,
                MaxUsers = 2,
                TrialDays = 5,
                IsCustom = false,
                IsActive = true,
                Features = new PlanFeatures
                {
                    HasInternalChat = true,
                    HasFlowChatbot = true,
                    HasAiChatbot = true,
                    CanSendCampaigns = true,
                    MonthlyCampaignMessagesLimit = 1000, // Exemplo de limite
                    CanImportChats = true,
                    HasDashboards = true,
                    HasMessageShortcuts = true,
                    HasDepartments = true,
                    HasApiAccess = true, // A imagem mostra check no básico
                    HasTrelloIntegration = true
                }
            },

            // --- PLANO PROFISSIONAL ---
            new Plan
            {
                Id = Guid.NewGuid(),
                Name = "Profissional",
                Price = 159.90m,
                MaxUsers = 5,
                TrialDays = 5,
                IsCustom = false,
                IsActive = true,
                Features = new PlanFeatures
                {
                    HasInternalChat = true,
                    HasFlowChatbot = true,
                    HasAiChatbot = true,
                    CanSendCampaigns = true,
                    MonthlyCampaignMessagesLimit = 10000, // Limite maior
                    CanImportChats = true,
                    HasDashboards = true,
                    HasMessageShortcuts = true,
                    HasDepartments = true,
                    HasApiAccess = true,
                    HasTrelloIntegration = true
                }
            },

            // --- PLANO CUSTOMIZADO ---
            new Plan
            {
                Id = Guid.NewGuid(),
                Name = "Customizado",
                Price = 0m, // Sob consulta
                MaxUsers = 9999, // Sob demanda
                TrialDays = 0,
                IsCustom = true,
                IsActive = true,
                Features = new PlanFeatures
                {
                    HasInternalChat = true,
                    HasFlowChatbot = true,
                    HasAiChatbot = true,
                    CanSendCampaigns = true,
                    MonthlyCampaignMessagesLimit = int.MaxValue,
                    CanImportChats = true,
                    HasDashboards = true,
                    HasMessageShortcuts = true,
                    HasDepartments = true,
                    HasApiAccess = true,
                    HasTrelloIntegration = true,
                    HasPrioritySupport = true // Diferencial implícito
                }
            }
        };

        await _context.Plans.InsertManyAsync(plans);
    }
}