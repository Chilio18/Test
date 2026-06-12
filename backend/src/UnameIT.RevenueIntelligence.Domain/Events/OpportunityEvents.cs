using UnameIT.RevenueIntelligence.Domain.Common;
using UnameIT.RevenueIntelligence.Domain.Enums;

namespace UnameIT.RevenueIntelligence.Domain.Events;

public record OpportunityStageChangedEvent(Guid OpportunityId, Guid TenantId,
    OpportunityStage OldStage, OpportunityStage NewStage) : DomainEvent;
