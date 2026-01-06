using OmniChat.Domain.Entities;

namespace OmniChat.Domain.Interfaces;

public interface IPlanEnforcementService
{
    Task<UserSubscription> GetSubscriptionAsync(Guid userId);
    Task RegisterUsageAsync(Guid userId);
}