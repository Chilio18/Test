using MediatR;

namespace UnameIT.RevenueIntelligence.Application.Features.Calls.Commands;

public record SyncCallToCrmCommand(
    Guid CallId,
    Guid TenantId,
    Guid CrmConnectionId,
    bool CreateNote = true,
    bool CreateTask = true,
    bool UpdateOpportunity = false
) : IRequest<SyncCallToCrmResult>;

public record SyncCallToCrmResult(bool Success, string? NoteId, string? TaskId, string? Error);
