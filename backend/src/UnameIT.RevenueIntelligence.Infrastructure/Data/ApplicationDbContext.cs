using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UnameIT.RevenueIntelligence.Domain.Common;
using UnameIT.RevenueIntelligence.Domain.Entities;
using UnameIT.RevenueIntelligence.Domain.Interfaces;

namespace UnameIT.RevenueIntelligence.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    private readonly ICurrentTenant? _currentTenant;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
        ICurrentTenant? currentTenant = null) : base(options)
    {
        _currentTenant = currentTenant;
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMembership> TeamMemberships => Set<TeamMembership>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<Call> Calls => Set<Call>();
    public DbSet<Recording> Recordings => Set<Recording>();
    public DbSet<Transcript> Transcripts => Set<Transcript>();
    public DbSet<TranscriptSegment> TranscriptSegments => Set<TranscriptSegment>();
    public DbSet<CallInsight> CallInsights => Set<CallInsight>();
    public DbSet<ActionItem> ActionItems => Set<ActionItem>();
    public DbSet<ScorecardTemplate> ScorecardTemplates => Set<ScorecardTemplate>();
    public DbSet<ScorecardCriterion> ScorecardCriteria => Set<ScorecardCriterion>();
    public DbSet<CallScorecard> CallScorecards => Set<CallScorecard>();
    public DbSet<CallScore> CallScores => Set<CallScore>();
    public DbSet<CrmConnection> CrmConnections => Set<CrmConnection>();
    public DbSet<CrmFieldMapping> CrmFieldMappings => Set<CrmFieldMapping>();
    public DbSet<CrmSyncJob> CrmSyncJobs => Set<CrmSyncJob>();
    public DbSet<CrmSyncLog> CrmSyncLogs => Set<CrmSyncLog>();
    public DbSet<EmbeddingDocument> EmbeddingDocuments => Set<EmbeddingDocument>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("revenue");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Soft-delete global filter for tenant entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(TenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(ApplyTenantFilter),
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, [modelBuilder, _currentTenant?.TenantId ?? Guid.Empty]);
            }
        }
    }

    private static void ApplyTenantFilter<T>(ModelBuilder builder, Guid tenantId)
        where T : TenantEntity
    {
        builder.Entity<T>().HasQueryFilter(e =>
            !e.IsDeleted && (tenantId == Guid.Empty || e.TenantId == tenantId));
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.SetAudit(_currentTenant?.UserEmail ?? "system");
            }
        }

        var result = await base.SaveChangesAsync(ct);
        await DispatchDomainEventsAsync();
        return result;
    }

    private async Task DispatchDomainEventsAsync()
    {
        // Domain events are dispatched via MediatR — see DomainEventInterceptor
        var entities = ChangeTracker.Entries<BaseEntity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }
    }
}
