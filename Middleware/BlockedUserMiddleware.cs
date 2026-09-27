using System.Security.Claims;
using KhojMarket.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace KhojMarket.Api.Middleware;

public class BlockedUserMiddleware
{
    private readonly RequestDelegate _next;
    public BlockedUserMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, KhojMarketDbContext db)
    {
        if (context.User.Identity?.IsAuthenticated == true &&
            Guid.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            var blocked = await db.Users.AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => x.IsBlocked)
                .SingleOrDefaultAsync();

            if (blocked)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { message = "This account is blocked. Please contact support." });
                return;
            }
        }
        await _next(context);
    }
}
