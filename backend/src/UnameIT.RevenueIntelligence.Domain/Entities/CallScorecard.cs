using UnameIT.RevenueIntelligence.Domain.Common;

namespace UnameIT.RevenueIntelligence.Domain.Entities;

public class CallScorecard : TenantEntity
{
    public Guid CallId { get; private set; }
    public Guid TemplateId { get; private set; }
    public Guid? ReviewerId { get; private set; }
    public decimal TotalScore { get; private set; }
    public decimal MaxPossibleScore { get; private set; }
    public decimal ScorePercentage => MaxPossibleScore > 0 ? TotalScore / MaxPossibleScore * 100 : 0;
    public bool IsAiGenerated { get; private set; }
    public bool IsManagerReviewed { get; private set; }
    public string? ManagerNotes { get; private set; }
    public DateTimeOffset? ReviewedAt { get; private set; }

    public Call? Call { get; private set; }
    public ScorecardTemplate? Template { get; private set; }
    public User? Reviewer { get; private set; }

    private readonly List<CallScore> _scores = new();
    public IReadOnlyCollection<CallScore> Scores => _scores.AsReadOnly();

    private CallScorecard() : base() { }

    public static CallScorecard Create(Guid tenantId, Guid callId, Guid templateId, bool isAiGenerated)
    {
        return new CallScorecard
        {
            TenantId = tenantId,
            CallId = callId,
            TemplateId = templateId,
            IsAiGenerated = isAiGenerated
        };
    }

    public void SetManagerReview(Guid reviewerId, string? notes)
    {
        ReviewerId = reviewerId;
        IsManagerReviewed = true;
        ManagerNotes = notes;
        ReviewedAt = DateTimeOffset.UtcNow;
    }

    public void RecalculateTotal(decimal total, decimal max)
    {
        TotalScore = total;
        MaxPossibleScore = max;
    }
}
