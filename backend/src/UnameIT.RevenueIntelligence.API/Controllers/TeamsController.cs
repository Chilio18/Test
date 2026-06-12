using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UnameIT.RevenueIntelligence.API.Controllers;

[Tags("Teams")]
public class TeamsController : BaseController
{
    [HttpGet]
    public IActionResult GetTeams() => Ok(Array.Empty<object>());

    [HttpPost]
    [Authorize(Policy = "Manager")]
    public IActionResult CreateTeam([FromBody] CreateTeamRequest request)
        => Created("", new { id = Guid.NewGuid(), name = request.Name });

    [HttpGet("{id:guid}/members")]
    public IActionResult GetMembers(Guid id) => Ok(Array.Empty<object>());

    [HttpPost("{id:guid}/members")]
    [Authorize(Policy = "Manager")]
    public IActionResult AddMember(Guid id, [FromBody] AddMemberRequest request) => NoContent();
}

public record CreateTeamRequest(string Name, string? Description, Guid? ManagerId);
public record AddMemberRequest(Guid UserId);
