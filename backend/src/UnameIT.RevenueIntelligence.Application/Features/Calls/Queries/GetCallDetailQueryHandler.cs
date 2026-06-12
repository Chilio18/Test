using MediatR;
using UnameIT.RevenueIntelligence.Application.Common.Interfaces;
using UnameIT.RevenueIntelligence.Application.DTOs.AI;
using UnameIT.RevenueIntelligence.Application.Features.Calls.Queries;
using UnameIT.RevenueIntelligence.Domain.Enums;
using UnameIT.RevenueIntelligence.Domain.Exceptions;
using UnameIT.RevenueIntelligence.Domain.Interfaces.Repositories;
using System.Text.Json;

namespace UnameIT.RevenueIntelligence.Application.Features.Calls.Queries;

public class GetCallDetailQueryHandler : IRequestHandler<GetCallDetailQuery, CallDetailDto?>
{
    private readonly IUnitOfWork _uow;
    private readonly IStorageService _storage;
    private readonly ICacheService _cache;

    public GetCallDetailQueryHandler(IUnitOfWork uow, IStorageService storage, ICacheService cache)
    {
        _uow = uow;
        _storage = storage;
        _cache = cache;
    }

    public async Task<CallDetailDto?> Handle(GetCallDetailQuery query, CancellationToken ct)
    {
        var cacheKey = $"call:{query.TenantId}:{query.CallId}";
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            var call = await _uow.Calls.GetWithDetailsAsync(query.CallId, query.TenantId, ct);
            if (call == null) return null;

            var recordings = new List<RecordingDto>();
            foreach (var rec in call.Recordings)
            {
                var url = await _storage.GetPresignedUrlAsync(rec.StorageKey, TimeSpan.FromHours(4), ct);
                recordings.Add(new RecordingDto(rec.Id, rec.IsVideo, rec.DurationSeconds, url));
            }

            var transcript = call.Transcripts.FirstOrDefault();
            TranscriptDto? transcriptDto = null;
            if (transcript != null)
            {
                transcriptDto = new TranscriptDto(
                    transcript.Segments.OrderBy(s => s.SequenceNumber)
                        .Select(s => new TranscriptSegmentResponseDto(
                            s.Speaker, s.StartTime, s.EndTime, s.Text))
                        .ToList());
            }

            var summaryInsight = call.Insights.FirstOrDefault();
            ConversationSummaryDto? summary = null;
            if (summaryInsight?.ExecutiveSummary != null)
            {
                summary = new ConversationSummaryDto(
                    summaryInsight.ExecutiveSummary,
                    summaryInsight.DetailedSummaryJson ?? "",
                    summaryInsight.MeetingRecap ?? "",
                    "",
                    75,
                    "Positive");
            }

            var insights = call.Insights
                .Where(i => i.Type != InsightType.Opportunity)
                .Select(i => new InsightDto(i.Type.ToString(), i.Content, i.Quote,
                    i.StartTime, i.Speaker, i.ConfidenceScore ?? 0.8m))
                .ToList();

            var actionItems = call.ActionItems
                .Select(a => new ActionItemResponseDto(a.Id, a.Title, a.Description,
                    a.Status.ToString(), null, a.DueDate, a.IsCrmSynced))
                .ToList();

            return new CallDetailDto(
                call.Id, call.Title, call.Status.ToString(), call.Type.ToString(),
                call.MeetingDate, call.DurationSeconds, call.Language,
                call.Account != null ? new AccountSummaryDto(call.Account.Id, call.Account.Name, call.Account.Website) : null,
                call.Opportunity != null ? new OpportunitySummaryDto(call.Opportunity.Id, call.Opportunity.Name, call.Opportunity.Stage.ToString(), call.Opportunity.Amount) : null,
                new UserSummaryDto(call.Owner!.Id, call.Owner.FullName, call.Owner.Email, call.Owner.AvatarUrl),
                recordings,
                transcriptDto,
                summary,
                insights,
                actionItems,
                null, null, null, null, null,
                call.CreatedAt);
        }, TimeSpan.FromMinutes(15), ct);
    }
}
