namespace UnameIT.RevenueIntelligence.API.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext ctx)
    {
        // Tenant can be resolved from:
        // 1. JWT claim (tenant_id)
        // 2. X-Tenant-ID header
        // 3. Subdomain
        // The ICurrentTenant service reads from HttpContext claims, which are set by JWT auth.
        // Additional resolution (header/subdomain) can be added here as fallback.

        if (ctx.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantHeader)
            && Guid.TryParse(tenantHeader, out var tenantId))
        {
            // Additional validation could occur here (check tenant is active, etc.)
        }

        await _next(ctx);
    }
}
