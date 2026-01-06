namespace OmniChat.Application.Services;

public class RegisterOrganizationUseCase
{
    public async Task ExecuteAsync(RegisterDto input)
    {
        // 1. Identificar o Plano escolhido (ex: Básico)
        var plan = await _db.Plans.Find(p => p.Name == input.PlanName).FirstOrDefaultAsync();

        // 2. Criar Organização
        var org = new Organization(input.CompanyName);
        org.Subscription.PlanId = plan.Id;
        org.Subscription.Status = SubscriptionStatus.Trialing;
        org.Subscription.TrialEndsAt = DateTime.UtcNow.AddDays(plan.TrialDays);

        // 3. Criar Usuário Admin
        var adminUser = new User(input.AdminPhone);
        // Vincula usuário à organização (na entidade User, adicione OrganizationId)
        adminUser.OrganizationId = org.Id; 
        adminUser.Role = UserRole.Admin;

        // 4. Atualizar lista de membros da Org
        org.MemberIds.Add(adminUser.Id);

        // 5. Persistência Transacional (MongoDB suporta transações em Replica Sets)
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