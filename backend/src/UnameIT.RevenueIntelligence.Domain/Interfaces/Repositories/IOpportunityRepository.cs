using UnameIT.RevenueIntelligence.Domain.Common;
using UnameIT.RevenueIntelligence.Domain.Entities;

namespace UnameIT.RevenueIntelligence.Domain.Interfaces.Repositories;

public interface IOpportunityRepository : IRepository<Opportunity>
{
    Task<Opportunity?> GetWithDetailsAsync(Guid id, Guid tenantId, CancellationToken ct = default);
    Task<PagedResult<Opportunity>> GetPagedAsync(Guid tenantId, int page, int pageSize,
        Guid? ownerId = null, Guid? accountId = null, bool? isStalled = null,
        CancellationToken ct = default);
    Task<IReadOnlyList<Opportunity>> GetHighRiskAsync(Guid tenantId, int riskThreshold = 70,
        CancellationToken ct = default);
}
