using MediatR;
using Microsoft.Extensions.Logging;
using UnameIT.RevenueIntelligence.Application.Common.Interfaces;
using UnameIT.RevenueIntelligence.Domain.Entities;
using UnameIT.RevenueIntelligence.Domain.Interfaces.Repositories;

namespace UnameIT.RevenueIntelligence.Application.Features.Calls.Commands;

public class UploadCallCommandHandler : IRequestHandler<UploadCallCommand, UploadCallResult>
{
    private readonly IUnitOfWork _uow;
    private readonly IStorageService _storage;
    private readonly IBackgroundJobService _jobs;
    private readonly ILogger<UploadCallCommandHandler> _logger;

    public UploadCallCommandHandler(IUnitOfWork uow, IStorageService storage,
        IBackgroundJobService jobs, ILogger<UploadCallCommandHandler> logger)
    {
        _uow = uow;
        _storage = storage;
        _jobs = jobs;
        _logger = logger;
    }

    public async Task<UploadCallResult> Handle(UploadCallCommand cmd, CancellationToken ct)
    {
        var call = Call.Create(cmd.TenantId, cmd.Title, cmd.MeetingDate, cmd.OwnerId, cmd.CallType);
        call.SetConsent(cmd.ConsentObtained, cmd.ConsentMethod);

        if (cmd.AccountId.HasValue) call.LinkToOpportunity(cmd.AccountId.Value);

        await _uow.Calls.AddAsync(call, ct);

        var storageKey = $"recordings/{cmd.TenantId}/{call.Id}/{Guid.NewGuid()}/{cmd.FileName}";
        var recording = Recording.Create(cmd.TenantId, call.Id, cmd.FileName,
            storageKey, cmd.ContentType, cmd.FileSizeBytes, cmd.IsVideo);

        var presignedUrl = await _storage.GetPresignedUrlAsync(storageKey, TimeSpan.FromHours(2), ct);

        await _uow.SaveChangesAsync(ct);

        await _jobs.EnqueueAsync(new ProcessCallJob(call.Id, cmd.TenantId, recording.Id), ct);

        _logger.LogInformation("Call {CallId} created for tenant {TenantId}", call.Id, cmd.TenantId);

        return new UploadCallResult(call.Id, recording.Id, presignedUrl);
    }
}

public record ProcessCallJob(Guid CallId, Guid TenantId, Guid RecordingId);
