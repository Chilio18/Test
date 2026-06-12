using Microsoft.AspNetCore.Mvc;
using UnameIT.RevenueIntelligence.Application.Features.Calls.Commands;
using UnameIT.RevenueIntelligence.Application.Features.Calls.Queries;
using UnameIT.RevenueIntelligence.Domain.Common;
using UnameIT.RevenueIntelligence.Domain.Enums;

namespace UnameIT.RevenueIntelligence.API.Controllers;

/// <summary>Revenue calls management</summary>
[Tags("Calls")]
public class CallsController : BaseController
{
    /// <summary>Get paginated list of calls</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CallListItemDto>), 200)]
    public async Task<IActionResult> GetCalls(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? ownerId = null,
        [FromQuery] Guid? accountId = null,
        [FromQuery] CallStatus? status = null,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetCallsQuery(
            TenantId, page, pageSize, ownerId, accountId, status, from, to), ct);
        return Ok(result);
    }

    /// <summary>Get call detail with transcript, insights, and analysis</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CallDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCall(Guid id, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetCallDetailQuery(id, TenantId), ct);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>Initiate a call upload — returns presigned upload URL</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UploadCallResult), 201)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> UploadCall(
        [FromBody] UploadCallRequest request, CancellationToken ct = default)
    {
        var cmd = new UploadCallCommand(
            TenantId,
            UserId ?? Guid.Empty,
            request.Title,
            request.MeetingDate,
            request.CallType,
            request.FileName,
            request.ContentType,
            request.FileSizeBytes,
            request.IsVideo,
            request.ConsentObtained,
            request.ConsentMethod,
            request.Language,
            request.AccountId,
            request.OpportunityId,
            request.Description);

        var result = await Mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetCall), new { id = result.CallId }, result);
    }

    /// <summary>Sync call insights to CRM</summary>
    [HttpPost("{id:guid}/sync-crm")]
    [ProducesResponseType(typeof(SyncCallToCrmResult), 200)]
    public async Task<IActionResult> SyncToCrm(
        Guid id,
        [FromBody] SyncCallToCrmRequest request,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new SyncCallToCrmCommand(
            id, TenantId, request.CrmConnectionId,
            request.CreateNote, request.CreateTask, request.UpdateOpportunity), ct);
        return Ok(result);
    }
}

public record UploadCallRequest(
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
);

public record SyncCallToCrmRequest(
    Guid CrmConnectionId,
    bool CreateNote = true,
    bool CreateTask = true,
    bool UpdateOpportunity = false
);
