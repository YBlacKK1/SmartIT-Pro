using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartIT.Application;

namespace SmartIT.Web.Controllers;

[Authorize]
public sealed class HomeController(IDashboardService dashboardService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await dashboardService.GetAsync(cancellationToken));

    [AllowAnonymous]
    public IActionResult Error() => View();

    [AllowAnonymous]
    public IActionResult StatusCode(int code)
    {
        ViewBag.StatusCode = code;
        return View();
    }
}
