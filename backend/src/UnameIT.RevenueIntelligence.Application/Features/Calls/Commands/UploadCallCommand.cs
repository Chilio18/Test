using MediatR;
using UnameIT.RevenueIntelligence.Application.Common.Behaviors;
using UnameIT.RevenueIntelligence.Domain.Enums;

namespace UnameIT.RevenueIntelligence.Application.Features.Calls.Commands;

public record UploadCallCommand(
    Guid TenantId,
    Guid OwnerId,
    string Title,
    DateTimeOffset MeetingDate,
    CallType CallType,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    bool IsVideo,
    bool ConsentObtained,
    string? ConsentMethod,
    string? Language,
    Guid? AccountId,
    Guid? OpportunityId,
    string? Description
) : IRequest<UploadCallResult>, ITenantRequest;

public record UploadCallResult(Guid CallId, Guid RecordingId, string PresignedUploadUrl);
