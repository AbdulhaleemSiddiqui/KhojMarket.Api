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


    [HttpPatch("{dealId:guid}/seller-confirm")]
    [Authorize(Roles = "seller")]
    public async Task<IActionResult> ConfirmSellerCompletion(Guid dealId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await _dealService.ConfirmSellerCompletionAsync(userId, dealId));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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