using OmniChat.Domain.Entities;
using OmniChat.Domain.Interfaces;
using OmniChat.Infrastructure.Persistence;

namespace OmniChat.Application.Services;

public class PlanEnforcementService : IPlanEnforcementService
{
    private readonly AppDbContext _context;

    public PlanEnforcementService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserSubscription> GetSubscriptionAsync(Guid userId)
    {
        var subscription = await _context.UserSubscriptions
            .Include(u => u.Plan)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (subscription == null)
            throw new Exception("Usuário sem assinatura ativa.");

        // Lógica de Reset Mensal (Simplificada)
        // Idealmente, isso seria feito por um Background Service (Quartz.NET ou Hangfire)
        if (subscription.LastResetDate.Month != DateTime.UtcNow.Month)
        {
            subscription.MessagesUsedThisMonth = 0;
            subscription.LastResetDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return subscription;
    }

    public async Task RegisterUsageAsync(Guid userId)
    {
        var subscription = await _context.UserSubscriptions.FindAsync(userId);
        if (subscription != null)
        {
            subscription.MessagesUsedThisMonth++;
            await _context.SaveChangesAsync();
        }
    }
}