using System.Security.Claims;
using KhojMarket.Api.DTOs;
using KhojMarket.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KhojMarket.Api.Controllers;

[ApiController]
public class MarketplaceReviewsController : ControllerBase
{
    private readonly MarketplaceReviewService
        _reviewService;

    public MarketplaceReviewsController(
        MarketplaceReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [Authorize]
    [HttpPost("api/deals/{dealId:guid}/reviews")]
    public async Task<IActionResult> Create(
        Guid dealId,
        [FromBody] CreateMarketplaceReviewRequest request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result =
                await _reviewService.CreateAsync(
                    userId,
                    dealId,
                    request);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new
                {
                    message = ex.Message
                });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [AllowAnonymous]
    [HttpGet("api/users/{userId:guid}/rating")]
    public async Task<IActionResult> GetRating(
        Guid userId)
    {
        try
        {
            var result =
                await _reviewService
                    .GetUserRatingAsync(
                        userId);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    [AllowAnonymous]
    [HttpGet("api/users/{userId:guid}/reviews")]
    public async Task<IActionResult> GetUserReviews(
        Guid userId)
    {
        var result =
            await _reviewService
                .GetUserReviewsAsync(
                    userId);

        return Ok(result);
    }

    [Authorize]
    [HttpGet("api/deals/{dealId:guid}/reviews")]
    public async Task<IActionResult> GetDealReviews(
        Guid dealId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result =
                await _reviewService
                    .GetDealReviewsAsync(
                        userId,
                        dealId);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new
                {
                    message = ex.Message
                });
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