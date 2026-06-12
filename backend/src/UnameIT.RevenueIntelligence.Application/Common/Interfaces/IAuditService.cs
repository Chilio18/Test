using UnameIT.RevenueIntelligence.Domain.Enums;

namespace UnameIT.RevenueIntelligence.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAsync(AuditAction action, string entityType, string? entityId = null,
        object? oldValues = null, object? newValues = null, CancellationToken ct = default);
}
