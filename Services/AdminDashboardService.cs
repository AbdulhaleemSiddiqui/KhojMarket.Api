using KhojMarket.Api.Data;
using KhojMarket.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace KhojMarket.Api.Services;

public class AdminDashboardService
{
    private readonly KhojMarketDbContext _dbContext;

    public AdminDashboardService(
        KhojMarketDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminDashboardResponse> GetAsync()
    {
        var totalBuyers =
            await _dbContext.Users
                .AsNoTracking()
                .CountAsync(x =>
                    x.Role == "buyer");

        var totalSellers =
            await _dbContext.Users
                .AsNoTracking()
                .CountAsync(x =>
                    x.Role == "seller");

        var pendingSellers =
            await _dbContext.Users
                .AsNoTracking()
                .CountAsync(x =>
                    x.Role == "seller" &&
                    x.SellerVerificationStatus == "pending");

        var verifiedSellers =
            await _dbContext.Users
                .AsNoTracking()
                .CountAsync(x =>
                    x.Role == "seller" &&
                    x.SellerVerificationStatus == "verified");

        var totalRequirements =
            await _dbContext.Requirements
                .AsNoTracking()
                .CountAsync(x =>
                    !x.IsDeleted);

        var pendingRequirements =
            await _dbContext.Requirements
                .AsNoTracking()
                .CountAsync(x =>
                    !x.IsDeleted &&
                    x.Status == "admin_review_required");

        var activeVerifiedRequirements =
            await _dbContext.Requirements
                .AsNoTracking()
                .CountAsync(x =>
                    !x.IsDeleted &&
                    x.Status == "verified" &&
                    x.ActivityStatus == "active");

        var completedRequirements =
            await _dbContext.Requirements
                .AsNoTracking()
                .CountAsync(x =>
                    !x.IsDeleted &&
                    x.Status == "completed");

        var totalInquiries =
            await _dbContext.SellerInquiries
                .AsNoTracking()
                .CountAsync();

        var completedDeals =
            await _dbContext.MarketplaceDeals
                .AsNoTracking()
                .CountAsync(x =>
                    x.Status == "completed");

        var totalReviews =
            await _dbContext.MarketplaceReviews
                .AsNoTracking()
                .CountAsync();

        var totalSellerCreditBalance =
            await _dbContext.SellerWallets
                .AsNoTracking()
                .SumAsync(x =>
                    (int?)x.Balance)
            ?? 0;

        return new AdminDashboardResponse
        {
            TotalBuyers =
                totalBuyers,

            TotalSellers =
                totalSellers,

            PendingSellers =
                pendingSellers,

            VerifiedSellers =
                verifiedSellers,

            TotalRequirements =
                totalRequirements,

            PendingRequirements =
                pendingRequirements,

            ActiveVerifiedRequirements =
                activeVerifiedRequirements,

            CompletedRequirements =
                completedRequirements,

            TotalInquiries =
                totalInquiries,

            CompletedDeals =
                completedDeals,

            TotalReviews =
                totalReviews,

            TotalSellerCreditBalance =
                totalSellerCreditBalance
        };
    }
}