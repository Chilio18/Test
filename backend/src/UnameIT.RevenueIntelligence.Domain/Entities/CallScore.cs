using UnameIT.RevenueIntelligence.Domain.Common;

namespace UnameIT.RevenueIntelligence.Domain.Entities;

public class CallScore : TenantEntity
{
    public Guid ScorecardId { get; private set; }
    public Guid CriterionId { get; private set; }
    public decimal AiScore { get; private set; }
    public decimal? ManagerOverrideScore { get; private set; }
    public decimal EffectiveScore => ManagerOverrideScore ?? AiScore;
    public string? AiReasoning { get; private set; }
    public string? ManagerNotes { get; private set; }
    public string? SupportingQuote { get; private set; }
    public decimal? StartTime { get; private set; }

    public CallScorecard? Scorecard { get; private set; }
    public ScorecardCriterion? Criterion { get; private set; }

    private CallScore() : base() { }

    public static CallScore Create(Guid tenantId, Guid scorecardId, Guid criterionId,
        decimal aiScore, string? reasoning = null, string? quote = null)
    {
        return new CallScore
        {
            TenantId = tenantId,
            ScorecardId = scorecardId,
            CriterionId = criterionId,
            AiScore = aiScore,
            AiReasoning = reasoning,
            SupportingQuote = quote
        };
    }

    public void OverrideByManager(decimal score, string? notes)
    {
        ManagerOverrideScore = score;
        ManagerNotes = notes;
    }
}
