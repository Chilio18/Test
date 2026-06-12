using Microsoft.EntityFrameworkCore;
using UnameIT.RevenueIntelligence.Domain.Common;
using UnameIT.RevenueIntelligence.Domain.Entities;
using UnameIT.RevenueIntelligence.Domain.Interfaces.Repositories;

namespace UnameIT.RevenueIntelligence.Infrastructure.Data.Repositories;

public class OpportunityRepository : EfRepository<Opportunity>, IOpportunityRepository
{
    public OpportunityRepository(ApplicationDbContext db) : base(db) { }

    public async Task<Opportunity?> GetWithDetailsAsync(Guid id, Guid tenantId,
        CancellationToken ct = default)
        => await _db.Opportunities
            .Include(o => o.Account)
            .Include(o => o.Owner)
            .Include(o => o.Calls).ThenInclude(c => c.Owner)
            .Where(o => o.Id == id && o.TenantId == tenantId)
            .FirstOrDefaultAsync(ct);

    public async Task<PagedResult<Opportunity>> GetPagedAsync(Guid tenantId, int page, int pageSize,
        Guid? ownerId = null, Guid? accountId = null, bool? isStalled = null,
        CancellationToken ct = default)
    {
        var query = _db.Opportunities
            .Include(o => o.Account)
            .Include(o => o.Owner)
            .Where(o => o.TenantId == tenantId);

        if (ownerId.HasValue) query = query.Where(o => o.OwnerId == ownerId.Value);
        if (accountId.HasValue) query = query.Where(o => o.AccountId == accountId.Value);
        if (isStalled.HasValue) query = query.Where(o => o.IsStalled == isStalled.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(o => o.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Opportunity>(items, page, pageSize, total);
    }

    public async Task<IReadOnlyList<Opportunity>> GetHighRiskAsync(Guid tenantId,
        int riskThreshold = 70, CancellationToken ct = default)
        => await _db.Opportunities
            .Include(o => o.Account)
            .Include(o => o.Owner)
            .Where(o => o.TenantId == tenantId && o.RiskScore >= riskThreshold)
            .OrderByDescending(o => o.RiskScore)
            .ToListAsync(ct);
}
