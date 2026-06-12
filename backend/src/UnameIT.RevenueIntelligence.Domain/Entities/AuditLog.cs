using UnameIT.RevenueIntelligence.Domain.Common;
using UnameIT.RevenueIntelligence.Domain.Enums;

namespace UnameIT.RevenueIntelligence.Domain.Entities;

public class AuditLog : TenantEntity
{
    public Guid? UserId { get; private set; }
    public string? UserEmail { get; private set; }
    public AuditAction Action { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public string? EntityId { get; private set; }
    public string? OldValuesJson { get; private set; }
    public string? NewValuesJson { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? CorrelationId { get; private set; }
    public bool IsSuccess { get; private set; } = true;
    public string? ErrorMessage { get; private set; }

    private AuditLog() : base() { }

    public static AuditLog Create(Guid tenantId, Guid? userId, string? userEmail,
        AuditAction action, string entityType, string? entityId = null)
    {
        return new AuditLog
        {
            TenantId = tenantId,
            UserId = userId,
            UserEmail = userEmail,
            Action = action,
            EntityType = entityType,
            EntityId = entityId
        };
    }
}
