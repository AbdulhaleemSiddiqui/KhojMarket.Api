using System.Security.Claims;
using KhojMarket.Api.DTOs;
using KhojMarket.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KhojMarket.Api.Controllers;

[ApiController]
[Route("api/admin/requirements")]
[Authorize(Roles = "admin")]
public class AdminRequirementsController : ControllerBase
{
    private readonly RequirementVerificationService
        _verificationService;


    private readonly RequirementReadService
        _requirementReadService;
    public AdminRequirementsController(
        RequirementVerificationService verificationService,
        RequirementReadService requirementReadService)
    {
        _verificationService = verificationService;
        _requirementReadService = requirementReadService;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending(
    [FromQuery] RequirementListQueryRequest request)
    {
        var result =
            await _requirementReadService
                .GetPendingForAdminAsync(request);

        return Ok(result);
    }

    [HttpPost("{requirementId:guid}/approve")]
    public async Task<IActionResult> Approve(
        Guid requirementId)
    {
        if (!TryGetUserId(out var adminUserId))
        {
            return Unauthorized();
        }

        try
        {
            var result =
                await _verificationService.ApproveAsync(
                    requirementId,
                    "admin",
                    adminUserId);

            return Ok(result);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new
            {
                message = exception.Message
            });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }

    [HttpPost("{requirementId:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid requirementId,
        [FromBody] RejectRequirementRequest request)
    {
        if (!TryGetUserId(out var adminUserId))
        {
            return Unauthorized();
        }

        try
        {
            var result =
                await _verificationService.RejectAsync(
                    requirementId,
                    request.Reason,
                    "admin",
                    adminUserId);

            return Ok(result);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new
            {
                message = exception.Message
            });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
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