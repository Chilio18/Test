using MediatR;
using UnameIT.RevenueIntelligence.Domain.Interfaces;

namespace UnameIT.RevenueIntelligence.Application.Common.Behaviors;

public interface ITenantRequest
{
    Guid TenantId { get; }
}

public class TenantBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentTenant _currentTenant;

    public TenantBehavior(ICurrentTenant currentTenant) => _currentTenant = currentTenant;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (request is ITenantRequest tenantRequest &&
            tenantRequest.TenantId != _currentTenant.TenantId)
        {
            throw new UnauthorizedAccessException("Tenant mismatch detected.");
        }
        return await next();
    }
}
