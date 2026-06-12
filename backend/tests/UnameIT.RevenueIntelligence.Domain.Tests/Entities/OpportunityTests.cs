using FluentAssertions;
using UnameIT.RevenueIntelligence.Domain.Entities;
using UnameIT.RevenueIntelligence.Domain.Enums;
using Xunit;

namespace UnameIT.RevenueIntelligence.Domain.Tests.Entities;

public class OpportunityTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AccountId = Guid.NewGuid();

    [Fact]
    public void Create_ShouldStartAtProspecting()
    {
        var opp = Opportunity.Create(TenantId, "Big Deal", AccountId);

        opp.Stage.Should().Be(OpportunityStage.Prospecting);
        opp.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public void UpdateStage_ShouldRaiseStageChangedEvent()
    {
        var opp = Opportunity.Create(TenantId, "Test Deal", AccountId);
        opp.UpdateStage(OpportunityStage.Qualification);

        opp.Stage.Should().Be(OpportunityStage.Qualification);
        opp.DomainEvents.Should().ContainSingle(e => e is OpportunityStageChangedEvent);
    }

    [Fact]
    public void UpdateDealScore_ShouldSetAllScores()
    {
        var opp = Opportunity.Create(TenantId, "Scored Deal", AccountId);
        opp.UpdateDealScore(80, 20, 85, """{"risks":["no budget"]}""");

        opp.DealHealthScore.Should().Be(80);
        opp.RiskScore.Should().Be(20);
        opp.ConfidenceScore.Should().Be(85);
        opp.LastScoredAt.Should().NotBeNull();
    }
}
