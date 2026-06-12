using Microsoft.AspNetCore.Mvc;

namespace UnameIT.RevenueIntelligence.API.Controllers;

[Tags("CRM Integration")]
[Route("api/crm")]
public class CrmController : BaseController
{
    [HttpGet("connections")]
    public IActionResult GetConnections() => Ok(Array.Empty<object>());

    [HttpPost("connections/{id:guid}/test")]
    public IActionResult TestConnection(Guid id) => Ok(new { success = true, message = "Connection OK" });

    [HttpGet("connections/{id:guid}/sync-logs")]
    public IActionResult GetSyncLogs(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(new { items = Array.Empty<object>(), totalCount = 0 });

    [HttpPost("connections/{id:guid}/sync")]
    public IActionResult TriggerSync(Guid id) => Accepted(new { jobId = Guid.NewGuid() });
}
