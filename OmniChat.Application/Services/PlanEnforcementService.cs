using MongoDB.Driver;
using OmniChat.Domain.Entities;
using OmniChat.Domain.Interfaces;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories;

namespace OmniChat.Application.Services;

public class PlanEnforcementService
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
}