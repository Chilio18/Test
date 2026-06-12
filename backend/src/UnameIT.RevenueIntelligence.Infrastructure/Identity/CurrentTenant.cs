using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using UnameIT.RevenueIntelligence.Domain.Interfaces;

namespace UnameIT.RevenueIntelligence.Infrastructure.Identity;

public class CurrentTenant : ICurrentTenant
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentTenant(IHttpContextAccessor accessor) => _accessor = accessor;

    private ClaimsPrincipal? User => _accessor.HttpContext?.User;

    public Guid TenantId
    {
        get
        {
            var claim = User?.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }
    }

    public string TenantSlug => User?.FindFirst("tenant_slug")?.Value ?? string.Empty;

    public Guid? UserId
    {
        get
        {
            var claim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User?.FindFirst("sub")?.Value;
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }

    public string? UserEmail => User?.FindFirst(ClaimTypes.Email)?.Value
        ?? User?.FindFirst("email")?.Value;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
}
