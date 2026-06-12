namespace UnameIT.RevenueIntelligence.Domain.Common;

public abstract class TenantEntity : BaseEntity
{
    public Guid TenantId { get; protected set; }

    protected TenantEntity() { }
    protected TenantEntity(Guid tenantId) => TenantId = tenantId;
}
