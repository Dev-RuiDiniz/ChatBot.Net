namespace OmniChat.Domain.Entities;

public class UserSubscription
{
    public Guid UserId { get; set; }
    public int PlanId { get; set; }
    public Plan Plan { get; set; }
    public int MessagesUsedThisMonth { get; set; }
    
    public bool CanSendMessage() => MessagesUsedThisMonth < Plan.MonthlyMessageLimit;
}