using System.Security.Claims;
using KhojMarket.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KhojMarket.Api.Controllers;

[ApiController]
[Route("api/seller/wallet")]
[Authorize(Roles = "seller")]
public class SellerWalletController : ControllerBase
{
    private readonly SellerCreditService
        _creditService;

    public SellerWalletController(
        SellerCreditService creditService)
    {
        _creditService = creditService;
    }

    [HttpGet]
    public async Task<IActionResult> GetWallet()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result =
                await _creditService
                    .GetWalletAsync(userId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("transactions")]
    public async Task<IActionResult>
        GetTransactions()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result =
                await _creditService
                    .GetTransactionsAsync(
                        userId);

            return Ok(result);
        }
        catch (Exception ex)
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