using MongoDB.Driver;
using OmniChat.Domain.Entities; // Resolve 'User', 'Organization'
using OmniChat.Domain.Enums;    // Resolve 'UserRole', 'SubscriptionStatus'
using OmniChat.Infrastructure.Persistence; // Resolve 'MongoDbContext'
using OmniChat.Shared.DTOs;

namespace OmniChat.Application.Services;

public class RegisterOrganizationUseCase
{
    private readonly MongoDbContext _db;

    // Construtor para injeção de dependência (Resolve o erro do '_db')
    public RegisterOrganizationUseCase(MongoDbContext db)
    {
        _db = db;
    }

    public async Task ExecuteAsync(RegisterDto input)
    {
        // 1. Identificar o Plano escolhido
        var plan = await _db.Plans
            .Find(p => p.Name == input.PlanName)
            .FirstOrDefaultAsync();

        if (plan == null) throw new Exception("Plano não encontrado.");

        // 2. Criar Organização
        var org = new Organization(input.CompanyName);
        org.Subscription.PlanId = plan.Id;
        org.Subscription.Status = SubscriptionStatus.Trialing;
        org.Subscription.TrialEndsAt = DateTime.UtcNow.AddDays(plan.TrialDays);

        // 3. Criar Usuário Admin
        var adminUser = new User(input.AdminPhone);
        
        // Preenche dados adicionais do DTO
        adminUser.Email = input.AdminEmail; 
        // Nota: Em produção, use o AuthService.HashPassword(input.Password) aqui
        adminUser.PasswordHash = input.Password; 

        adminUser.OrganizationId = org.Id; 
        adminUser.Role = UserRole.OrganizationAdmin; // Admin da Empresa

        // 4. Atualizar lista de membros da Org
        org.MemberIds.Add(adminUser.Id);

        // 5. Persistência Transacional
        using var session = await _db.Client.StartSessionAsync();
        session.StartTransaction();
        try 
        {
            await _db.Organizations.InsertOneAsync(session, org);
            await _db.Users.InsertOneAsync(session, adminUser);
            await session.CommitTransactionAsync();
        }
        catch 
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }
}