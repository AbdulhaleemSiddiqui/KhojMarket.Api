using KhojMarket.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KhojMarket.Api.Controllers;

[ApiController]
[Route("api/admin/inquiries")]
[Authorize(Roles = "admin")]
public class AdminInquiriesController : ControllerBase
{
    private readonly SellerInquiryService _inquiryService;

    public AdminInquiriesController(
        SellerInquiryService inquiryService)
    {
        _inquiryService = inquiryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] string? status = null)
    {
        var result =
            await _inquiryService.GetAllForAdminAsync(
                search,
                status);

        return Ok(result);
    }
}
