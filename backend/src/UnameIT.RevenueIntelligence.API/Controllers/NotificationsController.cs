using Microsoft.AspNetCore.Mvc;

namespace UnameIT.RevenueIntelligence.API.Controllers;

[Tags("Notifications")]
public class NotificationsController : BaseController
{
    [HttpGet]
    public IActionResult GetNotifications([FromQuery] bool unreadOnly = false)
        => Ok(Array.Empty<object>());

    [HttpPost("{id:guid}/read")]
    public IActionResult MarkRead(Guid id) => NoContent();

    [HttpPost("read-all")]
    public IActionResult MarkAllRead() => NoContent();
}
