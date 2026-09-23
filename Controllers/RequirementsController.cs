using System.Security.Claims;
using KhojMarket.Api.DTOs;
using KhojMarket.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KhojMarket.Api.Controllers;

[ApiController]
[Route("api/requirements")]
public class RequirementsController : ControllerBase
{
    private readonly RequirementService
        _requirementService;

    private readonly RequirementReadService _requirementReadService;
    private readonly MarketplaceDealService _marketplaceDealService;

    private readonly RequirementLifecycleService
        _requirementLifecycleService;
    public RequirementsController(
    RequirementService requirementService,
    RequirementReadService requirementReadService,
    RequirementLifecycleService requirementLifecycleService, MarketplaceDealService marketplaceDealService)
    {
        _requirementService = requirementService;
        _requirementReadService = requirementReadService;
        _requirementLifecycleService = requirementLifecycleService;
        _marketplaceDealService = marketplaceDealService;
    }
    [HttpGet("{requirementId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDetails(
    Guid requirementId)
    {
        Guid? currentUserId = null;

        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (Guid.TryParse(
            userIdValue,
            out var parsedUserId))
        {
            currentUserId = parsedUserId;
        }

        try
        {
            var result =
                await _requirementLifecycleService
                    .GetDetailsAsync(
                        requirementId,
                        currentUserId);

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

    [HttpPatch("{requirementId:guid}/activity")]
    [Authorize(Roles = "buyer")]
    public async Task<IActionResult> UpdateActivity(
        Guid requirementId,
        [FromBody] UpdateRequirementActivityRequest request)
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
            userIdValue,
            out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result =
                await _requirementLifecycleService
                    .UpdateActivityAsync(
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

    [HttpPatch("{requirementId:guid}/complete")]
    [Authorize(Roles = "buyer")]
    public async Task<IActionResult> Complete(
        Guid requirementId,
        [FromBody] CompleteRequirementRequest request)
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
            userIdValue,
            out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result =
     await _marketplaceDealService
         .CompleteAsync(
             userId,
             requirementId,
             request);

            return Ok(new
            {
                message = "Requirement completed successfully.",
                deal = result
            });
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

    [HttpDelete("{requirementId:guid}")]
    [Authorize(Roles = "buyer")]
    public async Task<IActionResult> Delete(
        Guid requirementId,
        [FromBody] DeleteRequirementRequest request)
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
            userIdValue,
            out var userId))
        {
            return Unauthorized();
        }

        try
        {
            await _requirementLifecycleService
                .DeleteAsync(
                    userId,
                    requirementId,
                    request);

            return NoContent();
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

    [HttpPut("{requirementId:guid}")]
    [Authorize(Roles = "buyer")]
    public async Task<IActionResult> Update(
        Guid requirementId,
        [FromBody] UpdateRequirementRequest request)
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
            userIdValue,
            out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result =
                await _requirementLifecycleService
                    .UpdateAsync(
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
    [HttpGet("mine")]
    [Authorize(Roles = "buyer")]
    public async Task<IActionResult> GetMine(
    [FromQuery] RequirementListQueryRequest request)
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
            userIdValue,
            out var userId))
        {
            return Unauthorized();
        }

        var result =
            await _requirementReadService
                .GetMineAsync(
                    userId,
                    request);

        return Ok(result);
    }
    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublic(
    [FromQuery] RequirementListQueryRequest request)
    {
        var result =
            await _requirementReadService
                .GetPublicAsync(request);

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "buyer")]
    public async Task<IActionResult> Create(
    [FromBody] CreateRequirementRequest request)
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var requirement =
                await _requirementService.CreateAsync(
                    userId,
                    request);

            return Created(
                $"/api/requirements/{requirement.Id}",
                requirement);
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
    [HttpPost("{requirementId:guid}/images")]
    [Authorize(Roles = "buyer")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImages(
    Guid requirementId,
    [FromForm] UploadRequirementImagesRequest request)
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
            userIdValue,
            out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result =
                await _requirementService.UploadImagesAsync(
                    userId,
                    requirementId,
                    request.Images);

            return Ok(result);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new
            {
                message = exception.Message
            });
        }
        catch (UnauthorizedAccessException exception)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new
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
}