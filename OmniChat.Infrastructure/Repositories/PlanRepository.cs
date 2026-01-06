using MongoDB.Driver;
using OmniChat.Domain.Entities;
using OmniChat.Infrastructure.Persistence;

namespace OmniChat.Infrastructure.Repositories;

public class PlanRepository
{
    private readonly MongoDbContext _context;

    public PlanRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Plan?> GetByIdAsync(Guid id)
    {
        return await _context.Plans.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task CreatePlanAsync(Plan plan)
    {
        await _context.Plans.InsertOneAsync(plan);
    }
    
    // Cachear planos é uma boa prática, pois mudam pouco
    public async Task<List<Plan>> GetAllActivePlansAsync()
    {
        return await _context.Plans.Find(_ => true).ToListAsync();
    }
}