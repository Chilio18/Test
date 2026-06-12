using UnameIT.RevenueIntelligence.Domain.Common;
using UnameIT.RevenueIntelligence.Domain.Enums;

namespace UnameIT.RevenueIntelligence.Domain.Entities;

public class CrmSyncJob : TenantEntity
{
    public Guid ConnectionId { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public SyncStatus Status { get; private set; } = SyncStatus.Pending;
    public string? TriggerSource { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public int RecordsProcessed { get; private set; }
    public int RecordsSucceeded { get; private set; }
    public int RecordsFailed { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? MetadataJson { get; private set; }
    public int RetryCount { get; private set; }
    public DateTimeOffset? NextRetryAt { get; private set; }

    public CrmConnection? Connection { get; private set; }

    private readonly List<CrmSyncLog> _logs = new();
    public IReadOnlyCollection<CrmSyncLog> Logs => _logs.AsReadOnly();

    private CrmSyncJob() : base() { }

    public static CrmSyncJob Create(Guid tenantId, Guid connectionId, string entityType,
        string? triggerSource = null)
    {
        return new CrmSyncJob
        {
            TenantId = tenantId,
            ConnectionId = connectionId,
            EntityType = entityType,
            TriggerSource = triggerSource,
            StartedAt = DateTimeOffset.UtcNow
        };
    }

    public void Start()
    {
        Status = SyncStatus.Running;
        StartedAt = DateTimeOffset.UtcNow;
    }

    public void Complete(int processed, int succeeded, int failed)
    {
        Status = failed == 0 ? SyncStatus.Completed : SyncStatus.PartialSuccess;
        CompletedAt = DateTimeOffset.UtcNow;
        RecordsProcessed = processed;
        RecordsSucceeded = succeeded;
        RecordsFailed = failed;
    }

    public void Fail(string error)
    {
        Status = SyncStatus.Failed;
        CompletedAt = DateTimeOffset.UtcNow;
        ErrorMessage = error;
    }

    public void ScheduleRetry(DateTimeOffset nextRetry)
    {
        RetryCount++;
        NextRetryAt = nextRetry;
        Status = SyncStatus.Pending;
    }
}
