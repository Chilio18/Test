using MediatR;

namespace UnameIT.RevenueIntelligence.Domain.Common;

public abstract record DomainEvent : INotification
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
