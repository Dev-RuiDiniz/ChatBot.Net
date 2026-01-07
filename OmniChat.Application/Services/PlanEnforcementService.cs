using MongoDB.Driver;
using OmniChat.Domain.Entities;
using OmniChat.Domain.Enums;
using OmniChat.Domain.Interfaces;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories;

namespace OmniChat.Application.Services;

public class PlanEnforcementService : IPlanEnforcementService
{
    private readonly MongoDbContext _db;

    public PlanEnforcementService(MongoDbContext db)
    {
        _db = db;
    }

    // Validação de Feature Genérica
    public async Task<bool> CanAccessFeatureAsync(Guid organizationId, Func<PlanFeatures, bool> featureSelector)
    {
        // 1. Busca Organização com projeção leve
        var org = await _db.Organizations
            .Find(o => o.Id == organizationId)
            .FirstOrDefaultAsync();

        if (org == null) return false;

        // 2. Busca Plano (Idealmente do Cache)
        var plan = await _db.Plans
            .Find(p => p.Id == org.Subscription.PlanId)
            .FirstOrDefaultAsync();

        if (plan == null) return false;

        // 3. Verifica Status da Assinatura (Trial ou Ativo)
        bool isActive = org.Subscription.Status == SubscriptionStatus.Active;
        bool isTrial = org.Subscription.Status == SubscriptionStatus.Trialing 
                       && org.Subscription.TrialEndsAt > DateTime.UtcNow;

        if (!isActive && !isTrial) return false;

        // 4. Verifica a Feature específica
        return featureSelector(plan.Features);
    }

    // Validação Específica: Posso adicionar mais um usuário?
    public async Task<bool> CanAddUserAsync(Guid organizationId)
    {
        var org = await _db.Organizations.Find(o => o.Id == organizationId).FirstOrDefaultAsync();
        var plan = await _db.Plans.Find(p => p.Id == org.Subscription.PlanId).FirstOrDefaultAsync();

        // Se for -1 é ilimitado (Customizado), senão verifica a contagem atual
        if (plan.MaxUsers == -1) return true;

        return org.MemberIds.Count < plan.MaxUsers;
    }

    public async Task<UserSubscription> GetSubscriptionAsync(Guid userId)
    {
        // 1. Busca o usuário
        var user = await _db.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null) throw new Exception("Usuário não encontrado.");

        // 2. Determina o ID do plano (Direto do user ou da Organização)
        var planId = user.Subscription?.PlanId;

        // Se o usuário não tem plano direto, tenta pegar da Organização
        if (planId == null || planId == Guid.Empty)
        {
            if (user.OrganizationId.HasValue)
            {
                var org = await _db.Organizations.Find(o => o.Id == user.OrganizationId).FirstOrDefaultAsync();
                planId = org?.Subscription.PlanId;
            }
        }

        if (planId == null) throw new Exception("Usuário sem plano ativo.");

        // 3. Carrega o Plano
        var plan = await _db.Plans.Find(p => p.Id == planId).FirstOrDefaultAsync();
        
        // 4. Preenche a propriedade de navegação (importante para o Orquestrador)
        user.Subscription.Plan = plan; 
        
        return user.Subscription;
    }

    public async Task RegisterUsageAsync(Guid userId)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var update = Builders<User>.Update.Inc(u => u.Subscription.MessagesUsedThisMonth, 1);
        await _db.Users.UpdateOneAsync(filter, update);
    }
}