using UnameIT.RevenueIntelligence.Domain.Common;

namespace UnameIT.RevenueIntelligence.Domain.Entities;

public class ScorecardTemplate : TenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsDefault { get; private set; }
    public string? CallTypeFilter { get; private set; }
    public Guid? CreatedByUserId { get; private set; }

    private readonly List<ScorecardCriterion> _criteria = new();
    public IReadOnlyCollection<ScorecardCriterion> Criteria => _criteria.AsReadOnly();

    private readonly List<CallScorecard> _scorecards = new();
    public IReadOnlyCollection<CallScorecard> Scorecards => _scorecards.AsReadOnly();

    private ScorecardTemplate() : base() { }

    public static ScorecardTemplate Create(Guid tenantId, string name, Guid createdByUserId)
    {
        return new ScorecardTemplate
        {
            TenantId = tenantId,
            Name = name,
            CreatedByUserId = createdByUserId
        };
    }

    public void Update(string name, string? description, bool isActive)
    {
        Name = name;
        Description = description;
        IsActive = isActive;
    }

    public void SetDefault() => IsDefault = true;
    public void UnsetDefault() => IsDefault = false;
}
