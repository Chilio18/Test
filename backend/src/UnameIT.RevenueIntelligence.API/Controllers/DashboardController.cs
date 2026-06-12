using Microsoft.AspNetCore.Mvc;
using UnameIT.RevenueIntelligence.Application.Features.Dashboard.Queries;

namespace UnameIT.RevenueIntelligence.API.Controllers;

[Tags("Dashboard")]
public class DashboardController : BaseController
{
    [HttpGet("leadership")]
    [ProducesResponseType(typeof(LeadershipDashboardDto), 200)]
    public async Task<IActionResult> GetLeadershipDashboard(
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetLeadershipDashboardQuery(
            TenantId,
            from ?? DateTimeOffset.UtcNow.AddDays(-30),
            to ?? DateTimeOffset.UtcNow), ct);
        return Ok(result);
    }
}
