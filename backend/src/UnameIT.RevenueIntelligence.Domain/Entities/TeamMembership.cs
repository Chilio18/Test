using UnameIT.RevenueIntelligence.Domain.Common;

namespace UnameIT.RevenueIntelligence.Domain.Entities;

public class TeamMembership : TenantEntity
{
    public Guid TeamId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTimeOffset JoinedAt { get; private set; }

    public Team? Team { get; private set; }
    public User? User { get; private set; }

    private TeamMembership() : base() { }

    public static TeamMembership Create(Guid tenantId, Guid teamId, Guid userId)
    {
        return new TeamMembership
        {
            TenantId = tenantId,
            TeamId = teamId,
            UserId = userId,
            JoinedAt = DateTimeOffset.UtcNow
        };
    }
}
