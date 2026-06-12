using UnameIT.RevenueIntelligence.Application.DTOs.AI;

namespace UnameIT.RevenueIntelligence.Application.Common.Interfaces;

public interface IAIProvider
{
    string ProviderName { get; }
    string DefaultModel { get; }

    Task<ConversationSummaryDto> GenerateSummaryAsync(
        string transcript, string language, string meetingContext, CancellationToken ct = default);

    Task<IReadOnlyList<ActionItemDto>> ExtractActionItemsAsync(
        string transcript, string context, CancellationToken ct = default);

    Task<DealRiskAnalysisDto> AnalyzeDealRiskAsync(
        string transcript, OpportunityContextDto opportunity, CancellationToken ct = default);

    Task<CoachingFeedbackDto> GenerateCoachingFeedbackAsync(
        string transcript, ScorecardContextDto scorecard, CancellationToken ct = default);

    Task<CrmUpdateSuggestionsDto> GenerateCrmUpdateAsync(
        string transcript, CrmContextDto crmContext, CancellationToken ct = default);

    Task<string> AnswerQuestionAsync(
        string question, IReadOnlyList<string> contextDocuments, CancellationToken ct = default);

    Task<string> GenerateFollowUpEmailAsync(
        string transcript, FollowUpEmailContextDto context, CancellationToken ct = default);

    Task<IReadOnlyList<InsightDto>> ExtractInsightsAsync(
        string transcript, CancellationToken ct = default);

    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default);
}
