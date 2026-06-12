using UnameIT.RevenueIntelligence.Domain.Common;
using UnameIT.RevenueIntelligence.Domain.Enums;

namespace UnameIT.RevenueIntelligence.Domain.Entities;

public class Opportunity : TenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public Guid AccountId { get; private set; }
    public Guid? OwnerId { get; private set; }
    public OpportunityStage Stage { get; private set; }
    public decimal? Amount { get; private set; }
    public string? Currency { get; private set; } = "EUR";
    public DateTimeOffset? CloseDate { get; private set; }
    public decimal? Probability { get; private set; }
    public string? Description { get; private set; }
    public string? ExternalCrmId { get; private set; }
    public string? ExternalCrmProvider { get; private set; }
    public DateTimeOffset? LastCrmSyncAt { get; private set; }

    // Deal Intelligence Scores (0-100)
    public int? DealHealthScore { get; private set; }
    public int? RiskScore { get; private set; }
    public int? ConfidenceScore { get; private set; }
    public string? DealScoreExplanationJson { get; private set; }
    public DateTimeOffset? LastScoredAt { get; private set; }

    public bool HasNextStep { get; private set; }
    public bool HasBudgetDiscussion { get; private set; }
    public bool HasDecisionMaker { get; private set; }
    public bool IsStalled { get; private set; }
    public DateTimeOffset? LastActivityAt { get; private set; }

    public Account? Account { get; private set; }
    public User? Owner { get; private set; }

    private readonly List<Call> _calls = new();
    public IReadOnlyCollection<Call> Calls => _calls.AsReadOnly();

    private Opportunity() : base() { }

    public static Opportunity Create(Guid tenantId, string name, Guid accountId, Guid? ownerId = null)
    {
        return new Opportunity
        {
            TenantId = tenantId,
            Name = name,
            AccountId = accountId,
            OwnerId = ownerId,
            Stage = OpportunityStage.Prospecting
        };
    }

    public void UpdateStage(OpportunityStage stage)
    {
        var oldStage = Stage;
        Stage = stage;
        AddDomainEvent(new OpportunityStageChangedEvent(Id, TenantId, oldStage, stage));
    }

    public void UpdateDealScore(int health, int risk, int confidence, string explanationJson)
    {
        DealHealthScore = health;
        RiskScore = risk;
        ConfidenceScore = confidence;
        DealScoreExplanationJson = explanationJson;
        LastScoredAt = DateTimeOffset.UtcNow;
    }

    public void UpdateFlags(bool hasNextStep, bool hasBudget, bool hasDecisionMaker, bool isStalled)
    {
        HasNextStep = hasNextStep;
        HasBudgetDiscussion = hasBudget;
        HasDecisionMaker = hasDecisionMaker;
        IsStalled = isStalled;
        LastActivityAt = DateTimeOffset.UtcNow;
    }

    public void SetCrmMapping(string externalId, string provider)
    {
        ExternalCrmId = externalId;
        ExternalCrmProvider = provider;
        LastCrmSyncAt = DateTimeOffset.UtcNow;
    }
}
