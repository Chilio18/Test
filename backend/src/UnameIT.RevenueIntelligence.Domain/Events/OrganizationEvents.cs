using UnameIT.RevenueIntelligence.Domain.Common;

namespace UnameIT.RevenueIntelligence.Domain.Events;

public record OrganizationCreatedEvent(Guid OrganizationId, string Name) : DomainEvent;
public record TenantCreatedEvent(Guid TenantId, Guid OrganizationId, string Name) : DomainEvent;
