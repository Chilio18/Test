using MediatR;
using Microsoft.Extensions.Logging;
using UnameIT.RevenueIntelligence.Application.Common.Interfaces;
using UnameIT.RevenueIntelligence.Application.DTOs.AI;
using UnameIT.RevenueIntelligence.Application.DTOs.Speech;
using UnameIT.RevenueIntelligence.Domain.Entities;
using UnameIT.RevenueIntelligence.Domain.Enums;
using UnameIT.RevenueIntelligence.Domain.Exceptions;
using UnameIT.RevenueIntelligence.Domain.Interfaces.Repositories;

namespace UnameIT.RevenueIntelligence.Application.Features.Calls.Commands;

public class ProcessCallCommandHandler : IRequestHandler<ProcessCallCommand, bool>
{
    private readonly IUnitOfWork _uow;
    private readonly IStorageService _storage;
    private readonly ISpeechProvider _speech;
    private readonly IAIProvider _ai;
    private readonly IVectorSearchService _vectorSearch;
    private readonly INotificationService _notifications;
    private readonly ILogger<ProcessCallCommandHandler> _logger;

    public ProcessCallCommandHandler(IUnitOfWork uow, IStorageService storage,
        ISpeechProvider speech, IAIProvider ai, IVectorSearchService vectorSearch,
        INotificationService notifications, ILogger<ProcessCallCommandHandler> logger)
    {
        _uow = uow;
        _storage = storage;
        _speech = speech;
        _ai = ai;
        _vectorSearch = vectorSearch;
        _notifications = notifications;
        _logger = logger;
    }

    public async Task<bool> Handle(ProcessCallCommand cmd, CancellationToken ct)
    {
        var call = await _uow.Calls.GetWithDetailsAsync(cmd.CallId, cmd.TenantId, ct)
            ?? throw new NotFoundException(nameof(Call), cmd.CallId);

        try
        {
            call.StartProcessing();
            await _uow.SaveChangesAsync(ct);

            // Step 1: Transcribe
            call.StartTranscribing();
            await _uow.SaveChangesAsync(ct);

            var recording = call.Recordings.FirstOrDefault()
                ?? throw new DomainException("No recording found for call.");

            using var audioStream = await _storage.DownloadAsync(recording.StorageKey, ct);
            var transcriptionResult = await _speech.TranscribeAsync(
                audioStream,
                call.Language ?? "nl",
                new TranscriptionOptionsDto(EnableDiarization: true),
                ct);

            var transcript = Transcript.Create(cmd.TenantId, call.Id,
                transcriptionResult.Language, transcriptionResult.Provider);

            if (transcriptionResult.IsDiarized)
                transcript.SetDiarization(transcriptionResult.SpeakerCount);

            var fullText = string.Join("\n", transcriptionResult.Segments
                .Select(s => $"[{s.Speaker}] {s.Text}"));

            // Step 2: AI Analysis
            call.StartAnalyzing();
            await _uow.SaveChangesAsync(ct);

            var summaryTask = _ai.GenerateSummaryAsync(fullText, call.Language ?? "nl",
                call.Title, ct);
            var actionItemsTask = _ai.ExtractActionItemsAsync(fullText, call.Title, ct);
            var insightsTask = _ai.ExtractInsightsAsync(fullText, ct);

            await Task.WhenAll(summaryTask, actionItemsTask, insightsTask);

            var summary = await summaryTask;
            var actionItems = await actionItemsTask;
            var insights = await insightsTask;

            // Store summary as insight
            var summaryInsight = CallInsight.CreateSummary(cmd.TenantId, call.Id,
                summary.ExecutiveSummary, summary.DetailedSummary,
                summary.MeetingRecap, _ai.ProviderName, _ai.DefaultModel);

            // Store action items
            foreach (var item in actionItems)
            {
                var actionItem = ActionItem.Create(cmd.TenantId, call.Id, item.Title);
                await _uow.Calls.AddAsync(call, ct);
            }

            // Index for vector search
            await _vectorSearch.IndexDocumentAsync(
                call.Id.ToString(),
                await _ai.GenerateEmbeddingAsync(fullText, ct),
                fullText,
                new Dictionary<string, string>
                {
                    ["tenantId"] = cmd.TenantId.ToString(),
                    ["callId"] = call.Id.ToString(),
                    ["ownerId"] = call.OwnerId.ToString(),
                    ["accountId"] = call.AccountId?.ToString() ?? "",
                    ["date"] = call.MeetingDate.ToString("O")
                },
                ct);

            var totalDuration = (int)(transcriptionResult.Segments.LastOrDefault()?.EndTime ?? 0);
            call.Complete(totalDuration);
            await _uow.SaveChangesAsync(ct);

            await _notifications.SendAsync(cmd.TenantId, call.OwnerId, "call.completed",
                $"Analysis complete: {call.Title}",
                "Your call has been transcribed and analyzed.",
                $"/calls/{call.Id}", ct);

            _logger.LogInformation("Call {CallId} processed successfully", call.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process call {CallId}", call.Id);
            call.Fail(ex.Message);
            await _uow.SaveChangesAsync(ct);
            return false;
        }
    }
}
