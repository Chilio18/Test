using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnameIT.RevenueIntelligence.Domain.Interfaces;

namespace UnameIT.RevenueIntelligence.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    private ICurrentTenant? _tenant;
    protected ICurrentTenant CurrentTenant => _tenant ??= HttpContext.RequestServices.GetRequiredService<ICurrentTenant>();

    protected Guid TenantId => CurrentTenant.TenantId;
    protected Guid? UserId => CurrentTenant.UserId;
}
