using System.Security.Claims;
using KhojMarket.Api.DTOs;
using KhojMarket.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KhojMarket.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;

    public AuthController(
        UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        RegisterRequest request)
    {
        try
        {
            var result =
                await _userService.RegisterAsync(
                    request);

            return Created("", result);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        LoginRequest request)
    {
        var result =
            await _userService.LoginAsync(
                request);

        if (result is null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        return Ok(result);
    }

    [HttpPost("login-phone")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginByPhone(
        LoginByPhoneRequest request)
    {
        var result =
            await _userService.LoginByPhoneAsync(
                request);

        if (result is null)
        {
            return Unauthorized(new
            {
                message = "Invalid phone number or password."
            });
        }

        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
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

        var user =
            await _userService.GetByIdAsync(
                userId);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }
}
