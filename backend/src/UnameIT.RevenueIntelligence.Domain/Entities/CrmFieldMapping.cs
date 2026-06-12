using UnameIT.RevenueIntelligence.Domain.Common;

namespace UnameIT.RevenueIntelligence.Domain.Entities;

public class CrmFieldMapping : TenantEntity
{
    public Guid ConnectionId { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public string LocalField { get; private set; } = string.Empty;
    public string ExternalField { get; private set; } = string.Empty;
    public string? TransformExpression { get; private set; }
    public bool IsBidirectional { get; private set; }
    public bool IsRequired { get; private set; }
    public int SortOrder { get; private set; }

    public CrmConnection? Connection { get; private set; }

    private CrmFieldMapping() : base() { }

    public static CrmFieldMapping Create(Guid tenantId, Guid connectionId, string entityType,
        string localField, string externalField, bool bidirectional = false)
    {
        return new CrmFieldMapping
        {
            TenantId = tenantId,
            ConnectionId = connectionId,
            EntityType = entityType,
            LocalField = localField,
            ExternalField = externalField,
            IsBidirectional = bidirectional
        };
    }
}
