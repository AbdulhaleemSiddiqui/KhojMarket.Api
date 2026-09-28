using System.Security.Claims;
using KhojMarket.Api.DTOs;
using KhojMarket.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KhojMarket.Api.Controllers;

[ApiController]
[Route("api/trust")]
[Authorize]
public class MarketplaceTrustController : ControllerBase
{
    private readonly MarketplaceTrustService _service;
    public MarketplaceTrustController(MarketplaceTrustService service) => _service = service;

    [HttpPost("complaints")]
    public async Task<IActionResult> Report(CreateMarketplaceComplaintRequest request)
    {
        if (!TryUser(out var id)) return Unauthorized();
        try { return Ok(await _service.ReportAsync(id, request)); } catch(Exception ex) { return Error(ex); }
    }

    [HttpGet("admin/complaints")]
    [Authorize(Roles="admin")]
    public async Task<IActionResult> Complaints([FromQuery] string? status=null) => Ok(await _service.GetAdminComplaintsAsync(status));

    [HttpPatch("admin/complaints/{id:guid}/warning")]
    [Authorize(Roles="admin")]
    public async Task<IActionResult> Warning(Guid id, ReviewMarketplaceComplaintRequest request)
    {
        if (!TryUser(out var adminId)) return Unauthorized();
        try { return Ok(await _service.ConfirmWarningAsync(adminId,id,request)); } catch(Exception ex) { return Error(ex); }
    }

    [HttpPatch("admin/complaints/{id:guid}/dismiss")]
    [Authorize(Roles="admin")]
    public async Task<IActionResult> Dismiss(Guid id, ReviewMarketplaceComplaintRequest request)
    {
        if (!TryUser(out var adminId)) return Unauthorized();
        try { return Ok(await _service.DismissAsync(adminId,id,request)); } catch(Exception ex) { return Error(ex); }
    }

    [HttpGet("admin/blocked-users")]
    [Authorize(Roles="admin")]
    public async Task<IActionResult> Blocked() => Ok(await _service.GetBlockedUsersAsync());

    [HttpPatch("admin/blocked-users/{userId:guid}/unblock")]
    [Authorize(Roles="admin")]
    public async Task<IActionResult> Unblock(Guid userId)
    {
        try { await _service.UnblockAsync(userId); return NoContent(); } catch(Exception ex) { return Error(ex); }
    }

    private bool TryUser(out Guid id) => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out id);
    private IActionResult Error(Exception ex) => ex switch {
        KeyNotFoundException => NotFound(new {message=ex.Message}),
        UnauthorizedAccessException => StatusCode(403,new {message=ex.Message}),
        _ => BadRequest(new {message=ex.Message})
    };
}
