using Hangfire.Dashboard;

namespace UnameIT.RevenueIntelligence.API.Middleware;

public class HangfireAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.HasClaim("role", "PlatformAdmin");
    }
}
