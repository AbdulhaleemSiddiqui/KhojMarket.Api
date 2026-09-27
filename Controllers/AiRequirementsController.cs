using KhojMarket.Api.DTOs;
using KhojMarket.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KhojMarket.Api.Controllers;

[ApiController]
[Route("api/ai/requirements")]
public sealed class AiRequirementsController : ControllerBase
{
    private readonly IAiRequirementService _aiRequirementService;

    public AiRequirementsController(
        IAiRequirementService aiRequirementService)
    {
        _aiRequirementService = aiRequirementService;
    }

    [HttpPost("generate")]
    [AllowAnonymous]
    public async Task<IActionResult> Generate(
        [FromBody] GenerateAiRequirementRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest(new
            {
                message = "Requirement text is required."
            });
        }

        try
        {
            var result =
                await _aiRequirementService.GenerateAsync(
                    request.Text,
                    cancellationToken);

            return Ok(result);
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
