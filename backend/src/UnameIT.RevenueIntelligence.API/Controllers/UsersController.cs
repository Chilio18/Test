using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UnameIT.RevenueIntelligence.API.Controllers;

[Tags("Users")]
public class UsersController : BaseController
{
    [HttpGet]
    [Authorize(Policy = "Manager")]
    public IActionResult GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(new { items = Array.Empty<object>(), totalCount = 0 });

    [HttpGet("me")]
    public IActionResult GetMe() => Ok(new
    {
        userId = UserId,
        tenantId = TenantId,
        email = CurrentTenant.UserEmail
    });

    [HttpGet("{id:guid}")]
    public IActionResult GetUser(Guid id) => NotFound();
}
