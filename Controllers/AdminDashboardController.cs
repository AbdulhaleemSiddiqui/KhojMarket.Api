using KhojMarket.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KhojMarket.Api.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly AdminDashboardService
        _dashboardService;

    public AdminDashboardController(
        AdminDashboardService dashboardService)
    {
        _dashboardService =
            dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result =
            await _dashboardService
                .GetAsync();

        return Ok(result);
    }
}