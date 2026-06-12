using UnameIT.RevenueIntelligence.Domain.Common;
using UnameIT.RevenueIntelligence.Domain.Enums;

namespace UnameIT.RevenueIntelligence.Domain.Events;

public record CallCreatedEvent(Guid CallId, Guid TenantId, Guid OwnerId) : DomainEvent;
public record CallStatusChangedEvent(Guid CallId, Guid TenantId, CallStatus OldStatus, CallStatus NewStatus) : DomainEvent;
public record CallCompletedEvent(Guid CallId, Guid TenantId, Guid OwnerId) : DomainEvent;
public record CallFailedEvent(Guid CallId, Guid TenantId, string ErrorMessage) : DomainEvent;
