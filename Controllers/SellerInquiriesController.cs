using System.Security.Claims;
using KhojMarket.Api.DTOs;
using KhojMarket.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KhojMarket.Api.Controllers;

[ApiController]
[Route("api/inquiries")]
[Authorize]
public class SellerInquiriesController : ControllerBase
{
    private readonly SellerInquiryService
        _inquiryService;

    public SellerInquiriesController(
        SellerInquiryService inquiryService)
    {
        _inquiryService = inquiryService;
    }

    [HttpPost("requirements/{requirementId:guid}")]
    [Authorize(Roles = "seller")]
    public async Task<IActionResult> Send(
        Guid requirementId,
        [FromBody] CreateSellerInquiryRequest request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result =
                await _inquiryService.SendAsync(
                    userId,
                    requirementId,
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

    [HttpGet("requirement/{requirementId:guid}")]
    [Authorize(Roles = "buyer")]
    public async Task<IActionResult> GetForRequirement(
        Guid requirementId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result =
                await _inquiryService
                    .GetForBuyerRequirementAsync(
                        userId,
                        requirementId);

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

    [HttpGet("seller/mine")]
    [Authorize(Roles = "seller")]
    public async Task<IActionResult> GetSellerMine()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result =
            await _inquiryService
                .GetSellerMineAsync(userId);

        return Ok(result);
    }

    [HttpPatch("{inquiryId:guid}/viewed")]
    [Authorize(Roles = "buyer")]
    public async Task<IActionResult> MarkViewed(
        Guid inquiryId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result =
                await _inquiryService.MarkViewedAsync(
                    userId,
                    inquiryId);

            return Ok(result);
        }
        catch (Exception ex)
            when (
                ex is KeyNotFoundException ||
                ex is UnauthorizedAccessException)
        {
            return HandleException(ex);
        }
    }

    [HttpPatch("{inquiryId:guid}/accept")]
    [Authorize(Roles = "buyer")]
    public async Task<IActionResult> Accept(
        Guid inquiryId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result =
                await _inquiryService.AcceptAsync(
                    userId,
                    inquiryId);

            return Ok(result);
        }
        catch (Exception ex)
            when (
                ex is KeyNotFoundException ||
                ex is UnauthorizedAccessException ||
                ex is InvalidOperationException)
        {
            return HandleException(ex);
        }
    }

    [HttpPatch("{inquiryId:guid}/reject")]
    [Authorize(Roles = "buyer")]
    public async Task<IActionResult> Reject(
        Guid inquiryId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result =
                await _inquiryService.RejectAsync(
                    userId,
                    inquiryId);

            return Ok(result);
        }
        catch (Exception ex)
            when (
                ex is KeyNotFoundException ||
                ex is UnauthorizedAccessException ||
                ex is InvalidOperationException)
        {
            return HandleException(ex);
        }
    }

    private IActionResult HandleException(
        Exception exception)
    {
        return exception switch
        {
            KeyNotFoundException =>
                NotFound(new
                {
                    message = exception.Message
                }),

            UnauthorizedAccessException =>
                StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message = exception.Message
                    }),

            _ =>
                BadRequest(new
                {
                    message = exception.Message
                })
        };
    }

    private bool TryGetUserId(out Guid userId)
    {
        var value =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        return Guid.TryParse(
            value,
            out userId);
    }
}