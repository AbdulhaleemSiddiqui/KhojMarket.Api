using KhojMarket.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KhojMarket.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "admin")]
public class AdminUsersController : ControllerBase
{
    private readonly KhojMarketDbContext _dbContext;

    public AdminUsersController(
        KhojMarketDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _dbContext.Users
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            query = query.Where(x =>
                x.Name.Contains(term) ||
                x.Email.Contains(term) ||
                (x.Phone != null &&
                 x.Phone.Contains(term)) ||
                x.ReferenceNo.Contains(term));
        }

        var totalCount =
            await query.CountAsync();

        var items =
            await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    id = x.Id,
                    referenceNo = x.ReferenceNo,
                    name = x.Name,
                    email = x.Email,
                    phone = x.Phone,
                    role = x.Role,
                    phoneVerified = x.PhoneVerified,
                    sellerVerificationStatus =
                        x.SellerVerificationStatus,
                    createdAt = x.CreatedAt
                })
                .ToListAsync();

        return Ok(new
        {
            items,
            page,
            pageSize,
            totalCount,
            totalPages =
                (int)Math.Ceiling(
                    totalCount /
                    (double)pageSize)
        });
    }
}
