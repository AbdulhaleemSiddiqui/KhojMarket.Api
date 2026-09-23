using System.Security.Claims;
using KhojMarket.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KhojMarket.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/requirements/favorites")]
public class RequirementFavoritesController : ControllerBase
{
    private readonly RequirementFavoriteService
        _favoriteService;

    public RequirementFavoritesController(
        RequirementFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpPost("{requirementId:guid}")]
    public async Task<IActionResult> Add(
        Guid requirementId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result =
                await _favoriteService.AddAsync(
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
    }

    [HttpDelete("{requirementId:guid}")]
    public async Task<IActionResult> Remove(
        Guid requirementId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result =
            await _favoriteService.RemoveAsync(
                userId,
                requirementId);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetMine()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var ids =
            await _favoriteService
                .GetFavoriteIdsAsync(userId);

        return Ok(ids);
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