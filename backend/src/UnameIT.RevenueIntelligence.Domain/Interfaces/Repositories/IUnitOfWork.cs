namespace UnameIT.RevenueIntelligence.Domain.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    ICallRepository Calls { get; }
    IOpportunityRepository Opportunities { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
