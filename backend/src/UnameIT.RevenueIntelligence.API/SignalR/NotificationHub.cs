using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using UnameIT.RevenueIntelligence.Domain.Interfaces;

namespace UnameIT.RevenueIntelligence.API.SignalR;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ICurrentTenant _tenant;
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ICurrentTenant tenant, ILogger<NotificationHub> logger)
    {
        _tenant = tenant;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var tenantId = _tenant.TenantId.ToString();
        var userId = _tenant.UserId?.ToString() ?? "anonymous";
        await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant:{tenantId}");
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
        _logger.LogInformation("User {UserId} connected to tenant {TenantId}", userId, tenantId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinCallRoom(string callId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"call:{callId}");
    }

    public async Task LeaveCallRoom(string callId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"call:{callId}");
    }
}
