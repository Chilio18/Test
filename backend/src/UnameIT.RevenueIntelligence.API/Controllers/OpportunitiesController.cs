using Microsoft.AspNetCore.Mvc;
using UnameIT.RevenueIntelligence.Application.Features.Dashboard.Queries;

namespace UnameIT.RevenueIntelligence.API.Controllers;

[Tags("Opportunities")]
public class OpportunitiesController : BaseController
{
    [HttpGet]
    public IActionResult GetOpportunities(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] Guid? ownerId = null, [FromQuery] bool? isStalled = null)
        => Ok(new { items = Array.Empty<object>(), page, pageSize, totalCount = 0 });

    [HttpGet("{id:guid}")]
    public IActionResult GetOpportunity(Guid id) => NotFound();

    [HttpGet("{id:guid}/deal-risk")]
    public IActionResult GetDealRisk(Guid id) => Ok(new { riskScore = 0 });
}
