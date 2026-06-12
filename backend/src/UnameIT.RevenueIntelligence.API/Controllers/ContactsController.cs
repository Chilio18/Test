using Microsoft.AspNetCore.Mvc;

namespace UnameIT.RevenueIntelligence.API.Controllers;

[Tags("Contacts")]
public class ContactsController : BaseController
{
    [HttpGet]
    public IActionResult GetContacts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(new { items = Array.Empty<object>(), page, pageSize, totalCount = 0 });

    [HttpGet("{id:guid}")]
    public IActionResult GetContact(Guid id) => NotFound();
}
