using UnameIT.RevenueIntelligence.Domain.Common;

namespace UnameIT.RevenueIntelligence.Domain.Entities;

public class CrmSyncLog : TenantEntity
{
    public Guid JobId { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public string? ExternalId { get; private set; }
    public string? LocalId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? RequestPayloadJson { get; private set; }
    public string? ResponsePayloadJson { get; private set; }
    public int? HttpStatusCode { get; private set; }
    public DateTimeOffset ProcessedAt { get; private set; }

    public CrmSyncJob? Job { get; private set; }

    private CrmSyncLog() : base() { }

    public static CrmSyncLog CreateSuccess(Guid tenantId, Guid jobId, string entityType,
        string action, string? externalId, string? localId)
    {
        return new CrmSyncLog
        {
            TenantId = tenantId,
            JobId = jobId,
            EntityType = entityType,
            Action = action,
            ExternalId = externalId,
            LocalId = localId,
            IsSuccess = true,
            ProcessedAt = DateTimeOffset.UtcNow
        };
    }

    public static CrmSyncLog CreateFailure(Guid tenantId, Guid jobId, string entityType,
        string action, string error, string? localId = null)
    {
        return new CrmSyncLog
        {
            TenantId = tenantId,
            JobId = jobId,
            EntityType = entityType,
            Action = action,
            LocalId = localId,
            IsSuccess = false,
            ErrorMessage = error,
            ProcessedAt = DateTimeOffset.UtcNow
        };
    }
}
