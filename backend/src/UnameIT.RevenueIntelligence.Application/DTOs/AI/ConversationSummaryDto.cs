namespace UnameIT.RevenueIntelligence.Application.DTOs.AI;

public record ConversationSummaryDto(
    string ExecutiveSummary,
    string DetailedSummary,
    string MeetingRecap,
    string KeyTopics,
    int SentimentScore,
    string SentimentLabel
);

public record ActionItemDto(
    string Title,
    string? Description,
    string? AssigneeName,
    string? DueDate,
    int Priority,
    string? Quote
);

public record InsightDto(
    string Type,
    string Content,
    string? Quote,
    decimal? StartTime,
    string? Speaker,
    decimal Confidence
);

public record DealRiskAnalysisDto(
    int HealthScore,
    int RiskScore,
    int ConfidenceScore,
    IReadOnlyList<string> Risks,
    IReadOnlyList<string> Opportunities,
    bool HasNextStep,
    bool HasBudgetDiscussion,
    bool HasDecisionMaker,
    string Explanation
);

public record CoachingFeedbackDto(
    int DiscoveryScore,
    int QualificationScore,
    int ObjectionHandlingScore,
    int ClosingScore,
    int CommunicationScore,
    int OverallScore,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> ImprovementAreas,
    string DetailedFeedback
);

public record CrmUpdateSuggestionsDto(
    string SuggestedNote,
    string? SuggestedNextStep,
    string? SuggestedStageChange,
    IReadOnlyList<string> SuggestedTasks,
    string? SuggestedDealUpdate
);

public record OpportunityContextDto(
    string OpportunityName,
    string AccountName,
    string Stage,
    decimal? Amount,
    DateTimeOffset? CloseDate,
    IReadOnlyList<string> PreviousCallSummaries
);

public record ScorecardContextDto(
    string TemplateName,
    IReadOnlyList<string> Criteria
);

public record CrmContextDto(
    string AccountName,
    string? ContactName,
    string? OpportunityName,
    string? CurrentStage
);

public record FollowUpEmailContextDto(
    string RecipientName,
    string RecipientEmail,
    string SenderName,
    string AccountName,
    string? OpportunityName
);
