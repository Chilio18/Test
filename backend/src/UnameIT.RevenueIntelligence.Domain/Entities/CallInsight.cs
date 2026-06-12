using UnameIT.RevenueIntelligence.Domain.Common;
using UnameIT.RevenueIntelligence.Domain.Enums;

namespace UnameIT.RevenueIntelligence.Domain.Entities;

public class CallInsight : TenantEntity
{
    public Guid CallId { get; private set; }
    public InsightType Type { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public string? Quote { get; private set; }
    public decimal? StartTime { get; private set; }
    public decimal? EndTime { get; private set; }
    public string? Speaker { get; private set; }
    public decimal? ConfidenceScore { get; private set; }
    public string? AiProvider { get; private set; }
    public string? AiModel { get; private set; }
    public string? MetadataJson { get; private set; }

    // Structured AI output fields
    public string? ExecutiveSummary { get; private set; }
    public string? DetailedSummaryJson { get; private set; }
    public string? MeetingRecap { get; private set; }
    public string? CoachingFeedbackJson { get; private set; }
    public string? CrmSuggestionsJson { get; private set; }
    public string? DealRiskAnalysisJson { get; private set; }
    public string? FollowUpEmailDraft { get; private set; }

    public Call? Call { get; private set; }

    private CallInsight() : base() { }

    public static CallInsight Create(Guid tenantId, Guid callId, InsightType type,
        string content, string? quote = null, decimal? confidence = null)
    {
        return new CallInsight
        {
            TenantId = tenantId,
            CallId = callId,
            Type = type,
            Content = content,
            Quote = quote,
            ConfidenceScore = confidence
        };
    }

    public static CallInsight CreateSummary(Guid tenantId, Guid callId, string executive,
        string detailed, string recap, string aiProvider, string aiModel)
    {
        return new CallInsight
        {
            TenantId = tenantId,
            CallId = callId,
            Type = InsightType.Opportunity,
            Content = "AI Generated Summary",
            ExecutiveSummary = executive,
            DetailedSummaryJson = detailed,
            MeetingRecap = recap,
            AiProvider = aiProvider,
            AiModel = aiModel
        };
    }
}
