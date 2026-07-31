using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartIT.Application;

namespace SmartIT.API.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public sealed class DashboardController(IDashboardService service) : ControllerBase
{
    [HttpGet]
    public Task<DashboardDto> Get(CancellationToken cancellationToken) =>
        service.GetAsync(cancellationToken);
}
