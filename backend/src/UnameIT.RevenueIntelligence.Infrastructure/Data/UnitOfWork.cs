using Microsoft.EntityFrameworkCore.Storage;
using UnameIT.RevenueIntelligence.Domain.Interfaces.Repositories;
using UnameIT.RevenueIntelligence.Infrastructure.Data.Repositories;

namespace UnameIT.RevenueIntelligence.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _db;
    private IDbContextTransaction? _transaction;

    public ICallRepository Calls { get; }
    public IOpportunityRepository Opportunities { get; }

    public UnitOfWork(ApplicationDbContext db)
    {
        _db = db;
        Calls = new CallRepository(db);
        Opportunities = new OpportunityRepository(db);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _transaction = await _db.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _db.Dispose();
    }
}
