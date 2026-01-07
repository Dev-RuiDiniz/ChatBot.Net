using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using OmniChat.Domain.Enums;

namespace OmniChat.Domain.Entities;

public class OrganizationSubscription
{
    [BsonRepresentation(BsonType.String)]
    public Guid PlanId { get; set; }
    
    [BsonIgnore]
    public Plan? CachedPlanSnapshot { get; set; }

    public SubscriptionStatus Status { get; set; }
    public DateTime? TrialEndsAt { get; set; }
    public DateTime CurrentPeriodEndsAt { get; set; }
}