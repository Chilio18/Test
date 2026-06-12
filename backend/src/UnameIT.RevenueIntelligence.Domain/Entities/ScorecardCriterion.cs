using UnameIT.RevenueIntelligence.Domain.Common;

namespace UnameIT.RevenueIntelligence.Domain.Entities;

public class ScorecardCriterion : TenantEntity
{
    public Guid TemplateId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? AiPromptHint { get; private set; }
    public int MaxScore { get; private set; } = 10;
    public int Weight { get; private set; } = 1;
    public int SortOrder { get; private set; }
    public bool IsRequired { get; private set; } = true;

    public ScorecardTemplate? Template { get; private set; }

    private ScorecardCriterion() : base() { }

    public static ScorecardCriterion Create(Guid tenantId, Guid templateId, string name,
        int maxScore = 10, int weight = 1, int sortOrder = 0)
    {
        return new ScorecardCriterion
        {
            TenantId = tenantId,
            TemplateId = templateId,
            Name = name,
            MaxScore = maxScore,
            Weight = weight,
            SortOrder = sortOrder
        };
    }
}
