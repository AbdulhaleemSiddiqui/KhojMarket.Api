using System.Security.Claims;
using KhojMarket.Api.DTOs;
using KhojMarket.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KhojMarket.Api.Controllers;

[ApiController]
[Route("api/admin/sellers")]
[Authorize(Roles = "admin")]
public class AdminSellersController : ControllerBase
{
    private readonly SellerVerificationService
        _sellerVerificationService;

    public AdminSellersController(
        SellerVerificationService sellerVerificationService)
    {
        _sellerVerificationService =
            sellerVerificationService;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var result =
            await _sellerVerificationService
                .GetPendingSellersAsync();

        return Ok(result);
    }

    [HttpGet("{sellerUserId:guid}")]
    public async Task<IActionResult> GetSeller(
        Guid sellerUserId)
    {
        try
        {
            var result =
                await _sellerVerificationService
                    .GetSellerAsync(
                        sellerUserId);

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

    [HttpPost("{sellerUserId:guid}/approve")]
    public async Task<IActionResult> Approve(
        Guid sellerUserId)
    {
        if (!TryGetUserId(out var adminUserId))
        {
            return Unauthorized();
        }

        try
        {
            var result =
                await _sellerVerificationService
                    .ApproveAsync(
                        adminUserId,
                        sellerUserId);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
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

    [HttpPost("{sellerUserId:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid sellerUserId,
        [FromBody] SellerVerificationRequest request)
    {
        if (!TryGetUserId(out var adminUserId))
        {
            return Unauthorized();
        }

        try
        {
            var result =
                await _sellerVerificationService
                    .RejectAsync(
                        adminUserId,
                        sellerUserId,
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
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