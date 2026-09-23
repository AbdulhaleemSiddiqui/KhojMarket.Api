using System.Security.Claims;
using KhojMarket.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KhojMarket.Api.Controllers;

[ApiController]
[Route("api/deals")]
[Authorize]
public class MarketplaceDealsController : ControllerBase
{
    private readonly MarketplaceDealService
        _dealService;

    public MarketplaceDealsController(
        MarketplaceDealService dealService)
    {
        _dealService = dealService;
    }

    [HttpGet("requirement/{requirementId:guid}")]
    public async Task<IActionResult>
        GetByRequirement(
            Guid requirementId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var deal =
            await _dealService
                .GetRequirementDealAsync(
                    requirementId,
                    userId);

        if (deal is null)
        {
            return NotFound(new
            {
                message =
                    "Deal not found."
            });
        }

        return Ok(deal);
    }

    private bool TryGetUserId(
        out Guid userId)
    {
        var value =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        return Guid.TryParse(
            value,
            out userId);
    }
}