using MediatR;
using UnameIT.RevenueIntelligence.Domain.Common;
using UnameIT.RevenueIntelligence.Domain.Enums;

namespace UnameIT.RevenueIntelligence.Application.Features.Calls.Queries;

public record GetCallsQuery(
    Guid TenantId,
    int Page = 1,
    int PageSize = 20,
    Guid? OwnerId = null,
    Guid? AccountId = null,
    CallStatus? Status = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null
) : IRequest<PagedResult<CallListItemDto>>;

public record CallListItemDto(
    Guid Id,
    string Title,
    string Status,
    string Type,
    DateTimeOffset MeetingDate,
    int? DurationSeconds,
    string OwnerName,
    string? AccountName,
    string? OpportunityName,
    int? DealHealthScore,
    int ActionItemCount,
    bool HasRecording
);
