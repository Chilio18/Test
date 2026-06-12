using UnameIT.RevenueIntelligence.Domain.Common;
using UnameIT.RevenueIntelligence.Domain.Enums;

namespace UnameIT.RevenueIntelligence.Domain.Events;

public record UserCreatedEvent(Guid UserId, Guid TenantId, string Email, UserRole Role) : DomainEvent;
public record UserRoleChangedEvent(Guid UserId, Guid TenantId, UserRole OldRole, UserRole NewRole) : DomainEvent;
