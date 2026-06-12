using Microsoft.AspNetCore.Mvc;

namespace UnameIT.RevenueIntelligence.API.Controllers;

/// <summary>CRM Accounts</summary>
[Tags("Accounts")]
public class AccountsController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAccounts(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, CancellationToken ct = default)
    {
        // TODO: wire up GetAccountsQuery
        return Ok(new { items = Array.Empty<object>(), page, pageSize, totalCount = 0 });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAccount(Guid id, CancellationToken ct = default)
    {
        return NotFound();
    }
}
