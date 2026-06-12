using UnameIT.RevenueIntelligence.Application.Common.Interfaces;
using UnameIT.RevenueIntelligence.Application.DTOs.AI;

namespace UnameIT.RevenueIntelligence.Infrastructure.AI.Mock;

public class MockAIProvider : IAIProvider
{
    public string ProviderName => "Mock";
    public string DefaultModel => "mock-model";

    public Task<ConversationSummaryDto> GenerateSummaryAsync(string transcript, string language,
        string meetingContext, CancellationToken ct = default)
        => Task.FromResult(new ConversationSummaryDto(
            "This was a productive discovery call.",
            "The customer expressed interest in the product and had questions about pricing.",
            "- Discussed product features\n- Pricing reviewed\n- Next steps agreed",
            "discovery, pricing, integration",
            72, "Positive"));

    public Task<IReadOnlyList<ActionItemDto>> ExtractActionItemsAsync(string transcript,
        string context, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<ActionItemDto>>(
        [
            new ActionItemDto("Send proposal", "Send detailed proposal by Friday", null, null, 1, null),
            new ActionItemDto("Schedule technical demo", null, null, null, 2, null)
        ]);

    public Task<DealRiskAnalysisDto> AnalyzeDealRiskAsync(string transcript,
        OpportunityContextDto opportunity, CancellationToken ct = default)
        => Task.FromResult(new DealRiskAnalysisDto(
            75, 25, 80,
            ["No signed NDA", "Budget not confirmed"],
            ["Champion identified", "Technical fit confirmed"],
            true, false, true,
            "Deal shows good progress with technical champion identified."));

    public Task<CoachingFeedbackDto> GenerateCoachingFeedbackAsync(string transcript,
        ScorecardContextDto scorecard, CancellationToken ct = default)
        => Task.FromResult(new CoachingFeedbackDto(8, 7, 6, 7, 8, 7,
            ["Strong rapport building", "Good discovery questions"],
            ["Could probe budget earlier", "Improve closing techniques"],
            "Good overall performance. Focus on confirming budget earlier in the process."));

    public Task<CrmUpdateSuggestionsDto> GenerateCrmUpdateAsync(string transcript,
        CrmContextDto context, CancellationToken ct = default)
        => Task.FromResult(new CrmUpdateSuggestionsDto(
            "Productive call. Customer interested in enterprise plan.",
            "Send proposal by end of week",
            "ProposalQuote",
            ["Follow up on technical requirements", "Send pricing sheet"],
            "Move to proposal stage"));

    public Task<string> AnswerQuestionAsync(string question,
        IReadOnlyList<string> contextDocuments, CancellationToken ct = default)
        => Task.FromResult($"Based on the conversations, the answer to '{question}' is: this is a mock response for development purposes.");

    public Task<string> GenerateFollowUpEmailAsync(string transcript,
        FollowUpEmailContextDto context, CancellationToken ct = default)
        => Task.FromResult($"Hi {context.RecipientName},\n\nThank you for your time today. As discussed, I'll send over the proposal by Friday.\n\nBest regards,\n{context.SenderName}");

    public Task<IReadOnlyList<InsightDto>> ExtractInsightsAsync(string transcript,
        CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<InsightDto>>(
        [
            new InsightDto("PainPoint", "Integration with existing systems is challenging", "integrating is always painful for us", 120m, "Customer", 0.9m),
            new InsightDto("BuyingSignal", "Customer wants to move forward this quarter", "we need this done by Q3", 240m, "Customer", 0.85m)
        ]);

    public Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        var random = new Random(text.GetHashCode());
        var vector = new float[1536];
        for (int i = 0; i < vector.Length; i++)
            vector[i] = (float)(random.NextDouble() * 2 - 1);
        return Task.FromResult(vector);
    }
}
