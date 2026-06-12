namespace UnameIT.RevenueIntelligence.Domain.Interfaces;

public interface ICurrentTenant
{
    Guid TenantId { get; }
    string TenantSlug { get; }
    Guid? UserId { get; }
    string? UserEmail { get; }
    bool IsAuthenticated { get; }
}
