namespace UnameIT.RevenueIntelligence.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendAsync(Guid tenantId, Guid recipientId, string type, string title,
        string? body = null, string? actionUrl = null, CancellationToken ct = default);

    Task SendToTeamAsync(Guid tenantId, Guid teamId, string type, string title,
        string? body = null, CancellationToken ct = default);
}
