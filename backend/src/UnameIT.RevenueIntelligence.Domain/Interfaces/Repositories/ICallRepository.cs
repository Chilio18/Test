using UnameIT.RevenueIntelligence.Domain.Common;
using UnameIT.RevenueIntelligence.Domain.Entities;
using UnameIT.RevenueIntelligence.Domain.Enums;

namespace UnameIT.RevenueIntelligence.Domain.Interfaces.Repositories;

public interface ICallRepository : IRepository<Call>
{
    Task<Call?> GetWithDetailsAsync(Guid id, Guid tenantId, CancellationToken ct = default);
    Task<PagedResult<Call>> GetPagedAsync(Guid tenantId, int page, int pageSize,
        Guid? ownerId = null, Guid? accountId = null, CallStatus? status = null,
        DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken ct = default);
    Task<IReadOnlyList<Call>> GetByOpportunityAsync(Guid opportunityId, Guid tenantId,
        CancellationToken ct = default);
    Task<int> GetCountByOwnerAsync(Guid ownerId, Guid tenantId, DateTimeOffset from,
        DateTimeOffset to, CancellationToken ct = default);
}
