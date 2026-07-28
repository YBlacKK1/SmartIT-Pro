using Microsoft.AspNetCore.Mvc; using SmartIT.Application;
namespace SmartIT.API.Controllers;
[ApiController][Microsoft.AspNetCore.Authorization.Authorize][Route("api/dashboard")] public class DashboardController(IDashboardService service):ControllerBase { [HttpGet] public Task<DashboardDto> Get(CancellationToken ct)=>service.GetAsync(ct); }
