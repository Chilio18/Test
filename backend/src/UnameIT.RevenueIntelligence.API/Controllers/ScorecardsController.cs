using Microsoft.AspNetCore.Mvc;

namespace UnameIT.RevenueIntelligence.API.Controllers;

[Tags("Scorecards")]
public class ScorecardsController : BaseController
{
    [HttpGet("templates")]
    public IActionResult GetTemplates() => Ok(Array.Empty<object>());

    [HttpPost("templates")]
    public IActionResult CreateTemplate([FromBody] object request) => Created("", new { id = Guid.NewGuid() });

    [HttpGet("{callId:guid}")]
    public IActionResult GetCallScorecard(Guid callId) => NotFound();
}
