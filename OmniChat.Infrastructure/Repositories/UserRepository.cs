using MongoDB.Driver;
using OmniChat.Domain.Entities;
using OmniChat.Infrastructure.Persistence;

namespace OmniChat.Infrastructure.Repositories;

public class UserRepository
{
    private readonly MongoDbContext _context;

    public UserRepository(MongoDbContext context)
    {
        _context = context;
    }
    
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Find(u => u.Email == email)
            .FirstOrDefaultAsync();
    }

    // O método abaixo também será útil para o PlanEnforcementService (passo 22 do histórico)
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Find(u => u.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
    {
        return await _context.Users
            .Find(u => u.PhoneNumber == phoneNumber)
            .FirstOrDefaultAsync();
    }

    public async Task CreateUserAsync(User user)
    {
        await _context.Users.InsertOneAsync(user);
    }

    public async Task UpdateSubscriptionUsageAsync(Guid userId, int newCount)
    {
        // Atualização atômica muito eficiente:
        // Aumenta o contador sem precisar ler o objeto inteiro e salvar de novo
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var update = Builders<User>.Update
            .Set(u => u.Subscription.MessagesUsedThisMonth, newCount);

        await _context.Users.UpdateOneAsync(filter, update);
    }

    // Método para associar um plano a um usuário
    public async Task SetPlanAsync(Guid userId, Guid planId)
    {
        var subscription = new UserSubscription
        {
            PlanId = planId,
            StartDate = DateTime.UtcNow,
            NextBillingDate = DateTime.UtcNow.AddMonths(1),
            MessagesUsedThisMonth = 0,
            LastUsageReset = DateTime.UtcNow
        };

        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var update = Builders<User>.Update.Set(u => u.Subscription, subscription);

        await _context.Users.UpdateOneAsync(filter, update);
    }
}