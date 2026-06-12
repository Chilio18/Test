using MediatR;
using UnameIT.RevenueIntelligence.Application.DTOs.AI;

namespace UnameIT.RevenueIntelligence.Application.Features.Calls.Queries;

public record GetCallDetailQuery(Guid CallId, Guid TenantId) : IRequest<CallDetailDto?>;

public record CallDetailDto(
    Guid Id,
    string Title,
    string Status,
    string Type,
    DateTimeOffset MeetingDate,
    int? DurationSeconds,
    string? Language,
    AccountSummaryDto? Account,
    OpportunitySummaryDto? Opportunity,
    UserSummaryDto Owner,
    IReadOnlyList<RecordingDto> Recordings,
    TranscriptDto? Transcript,
    ConversationSummaryDto? Summary,
    IReadOnlyList<InsightDto> Insights,
    IReadOnlyList<ActionItemResponseDto> ActionItems,
    DealRiskAnalysisDto? DealRisk,
    CoachingFeedbackDto? Coaching,
    CrmUpdateSuggestionsDto? CrmSuggestions,
    string? FollowUpEmail,
    ScorecardResponseDto? Scorecard,
    DateTimeOffset CreatedAt
);

public record AccountSummaryDto(Guid Id, string Name, string? Website);
public record OpportunitySummaryDto(Guid Id, string Name, string Stage, decimal? Amount);
public record UserSummaryDto(Guid Id, string FullName, string Email, string? AvatarUrl);
public record RecordingDto(Guid Id, bool IsVideo, int? DurationSeconds, string PresignedUrl);
public record TranscriptDto(IReadOnlyList<TranscriptSegmentResponseDto> Segments);
public record TranscriptSegmentResponseDto(string Speaker, decimal StartTime, decimal EndTime, string Text);
public record ActionItemResponseDto(Guid Id, string Title, string? Description, string Status, string? AssigneeName, DateTimeOffset? DueDate, bool IsCrmSynced);
public record ScorecardResponseDto(Guid Id, string TemplateName, decimal ScorePercentage, bool IsManagerReviewed, IReadOnlyList<ScoreItemDto> Scores);
public record ScoreItemDto(string CriterionName, decimal AiScore, decimal? ManagerScore, string? Reasoning);
