using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using SmartIT.Application;
namespace SmartIT.Web.Controllers;
[Authorize] public class HomeController(IDashboardService dashboard):Controller { public async Task<IActionResult> Index(CancellationToken ct)=>View(await dashboard.GetAsync(ct)); [AllowAnonymous] public IActionResult Error()=>View(); }
