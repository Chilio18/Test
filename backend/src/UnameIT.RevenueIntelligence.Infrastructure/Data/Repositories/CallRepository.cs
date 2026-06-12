using Microsoft.EntityFrameworkCore;
using UnameIT.RevenueIntelligence.Domain.Common;
using UnameIT.RevenueIntelligence.Domain.Entities;
using UnameIT.RevenueIntelligence.Domain.Enums;
using UnameIT.RevenueIntelligence.Domain.Interfaces.Repositories;

namespace UnameIT.RevenueIntelligence.Infrastructure.Data.Repositories;

public class CallRepository : EfRepository<Call>, ICallRepository
{
    public CallRepository(ApplicationDbContext db) : base(db) { }

    public async Task<Call?> GetWithDetailsAsync(Guid id, Guid tenantId, CancellationToken ct = default)
        => await _db.Calls
            .Include(c => c.Owner)
            .Include(c => c.Account)
            .Include(c => c.Opportunity)
            .Include(c => c.Recordings)
            .Include(c => c.Transcripts).ThenInclude(t => t.Segments.OrderBy(s => s.SequenceNumber))
            .Include(c => c.Insights)
            .Include(c => c.ActionItems).ThenInclude(a => a.Assignee)
            .Include(c => c.Scorecards).ThenInclude(s => s.Template)
            .Include(c => c.Scorecards).ThenInclude(s => s.Scores).ThenInclude(sc => sc.Criterion)
            .Where(c => c.Id == id && c.TenantId == tenantId)
            .FirstOrDefaultAsync(ct);

    public async Task<PagedResult<Call>> GetPagedAsync(Guid tenantId, int page, int pageSize,
        Guid? ownerId = null, Guid? accountId = null, CallStatus? status = null,
        DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken ct = default)
    {
        var query = _db.Calls
            .Include(c => c.Owner)
            .Include(c => c.Account)
            .Include(c => c.Opportunity)
            .Where(c => c.TenantId == tenantId);

        if (ownerId.HasValue) query = query.Where(c => c.OwnerId == ownerId.Value);
        if (accountId.HasValue) query = query.Where(c => c.AccountId == accountId.Value);
        if (status.HasValue) query = query.Where(c => c.Status == status.Value);
        if (from.HasValue) query = query.Where(c => c.MeetingDate >= from.Value);
        if (to.HasValue) query = query.Where(c => c.MeetingDate <= to.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(c => c.MeetingDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Call>(items, page, pageSize, total);
    }

    public async Task<IReadOnlyList<Call>> GetByOpportunityAsync(Guid opportunityId, Guid tenantId,
        CancellationToken ct = default)
        => await _db.Calls
            .Where(c => c.OpportunityId == opportunityId && c.TenantId == tenantId)
            .OrderByDescending(c => c.MeetingDate)
            .ToListAsync(ct);

    public async Task<int> GetCountByOwnerAsync(Guid ownerId, Guid tenantId,
        DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
        => await _db.Calls
            .CountAsync(c => c.OwnerId == ownerId && c.TenantId == tenantId &&
                c.MeetingDate >= from && c.MeetingDate <= to, ct);
}
