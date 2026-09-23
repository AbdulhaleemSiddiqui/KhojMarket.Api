using System.Security.Claims;
using KhojMarket.Api.DTOs;
using KhojMarket.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KhojMarket.Api.Controllers;

[ApiController]
[Route("api/admin/sellers")]
[Authorize(Roles = "admin")]
public class AdminSellerCreditsController : ControllerBase
{
    private readonly SellerCreditService
        _creditService;

    public AdminSellerCreditsController(
        SellerCreditService creditService)
    {
        _creditService = creditService;
    }

    [HttpGet("{sellerUserId:guid}/wallet")]
    public async Task<IActionResult> GetWallet(
        Guid sellerUserId)
    {
        try
        {
            var result =
                await _creditService
                    .GetSellerWalletForAdminAsync(
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

    [HttpGet("{sellerUserId:guid}/wallet/transactions")]
    public async Task<IActionResult> GetTransactions(
        Guid sellerUserId)
    {
        try
        {
            var result =
                await _creditService
                    .GetSellerTransactionsForAdminAsync(
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

    [HttpPost("{sellerUserId:guid}/wallet/adjust")]
    public async Task<IActionResult> AdjustCredits(
        Guid sellerUserId,
        [FromBody] AdminAdjustCreditsRequest request)
    {
        var adminUserIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
            adminUserIdValue,
            out var adminUserId))
        {
            return Unauthorized();
        }

        try
        {
            var result =
                await _creditService
                    .AdjustCreditsAsync(
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
}