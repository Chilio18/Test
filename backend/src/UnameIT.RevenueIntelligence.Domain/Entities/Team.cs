using UnameIT.RevenueIntelligence.Domain.Common;

namespace UnameIT.RevenueIntelligence.Domain.Entities;

public class Team : TenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? ManagerId { get; private set; }
    public bool IsActive { get; private set; } = true;

    public User? Manager { get; private set; }

    private readonly List<TeamMembership> _memberships = new();
    public IReadOnlyCollection<TeamMembership> Memberships => _memberships.AsReadOnly();

    private Team() : base() { }

    public static Team Create(Guid tenantId, string name, Guid? managerId = null)
    {
        return new Team
        {
            TenantId = tenantId,
            Name = name,
            ManagerId = managerId
        };
    }

    public void Update(string name, string? description, Guid? managerId)
    {
        Name = name;
        Description = description;
        ManagerId = managerId;
    }
}
