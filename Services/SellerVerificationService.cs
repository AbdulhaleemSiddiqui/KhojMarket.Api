using KhojMarket.Api.Data;
using KhojMarket.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace KhojMarket.Api.Services;

public class SellerVerificationService
{
    private readonly KhojMarketDbContext _dbContext;

    public SellerVerificationService(
        KhojMarketDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminSellerResponse>
        SubmitRequestAsync(
            Guid sellerUserId,
            SellerVerificationSubmissionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.BusinessName) ||
            string.IsNullOrWhiteSpace(request.City) ||
            string.IsNullOrWhiteSpace(request.Category))
        {
            throw new InvalidOperationException(
                "Business name, city and category are required.");
        }

        var seller = await _dbContext.Users
            .SingleOrDefaultAsync(x =>
                x.Id == sellerUserId &&
                x.Role == "seller");

        if (seller is null)
        {
            throw new KeyNotFoundException("Seller not found.");
        }

        if (seller.SellerVerificationStatus == "verified")
        {
            throw new InvalidOperationException(
                "This seller is already verified.");
        }

        seller.BusinessName = request.BusinessName.Trim();
        seller.BusinessCity = request.City.Trim();
        seller.BusinessCategory = request.Category.Trim();
        seller.SellerVerificationStatus = "pending";
        seller.SellerVerificationReason = null;
        seller.SellerVerifiedByUserId = null;
        seller.SellerVerifiedAt = null;
        seller.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return await GetSellerAsync(sellerUserId);
    }

    public async Task<List<AdminSellerResponse>>
        GetPendingSellersAsync()
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(x =>
                x.Role == "seller" &&
                x.SellerVerificationStatus == "pending")
            .OrderBy(x => x.CreatedAt)
            .Select(x => new AdminSellerResponse
            {
                Id = x.Id,
                ReferenceNo = x.ReferenceNo,
                Name = x.Name,
                Email = x.Email,
                Phone = x.Phone,
                PhoneVerified = x.PhoneVerified,
                BusinessName = x.BusinessName,
                BusinessCity = x.BusinessCity,
                BusinessCategory = x.BusinessCategory,
                SellerVerificationStatus =
                    x.SellerVerificationStatus,

                SellerVerificationReason =
                    x.SellerVerificationReason,

                SellerVerifiedAt =
                    x.SellerVerifiedAt,

                CreatedAt =
                    x.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<AdminSellerResponse>
        GetSellerAsync(
            Guid sellerUserId)
    {
        var seller =
            await _dbContext.Users
                .AsNoTracking()
                .Where(x =>
                    x.Id == sellerUserId &&
                    x.Role == "seller")
                .Select(x => new AdminSellerResponse
                {
                    Id = x.Id,
                    ReferenceNo = x.ReferenceNo,
                    Name = x.Name,
                    Email = x.Email,
                    Phone = x.Phone,
                    PhoneVerified = x.PhoneVerified,

                    BusinessName = x.BusinessName,
                    BusinessCity = x.BusinessCity,
                    BusinessCategory = x.BusinessCategory,

                    SellerVerificationStatus =
                        x.SellerVerificationStatus,

                    SellerVerificationReason =
                        x.SellerVerificationReason,

                    SellerVerifiedAt =
                        x.SellerVerifiedAt,

                    CreatedAt =
                        x.CreatedAt
                })
                .SingleOrDefaultAsync();

        if (seller is null)
        {
            throw new KeyNotFoundException(
                "Seller not found.");
        }

        return seller;
    }

    public async Task<AdminSellerResponse>
        ApproveAsync(
            Guid adminUserId,
            Guid sellerUserId)
    {
        var seller =
            await _dbContext.Users
                .SingleOrDefaultAsync(x =>
                    x.Id == sellerUserId &&
                    x.Role == "seller");

        if (seller is null)
        {
            throw new KeyNotFoundException(
                "Seller not found.");
        }

        if (seller.SellerVerificationStatus == "verified")
        {
            throw new InvalidOperationException(
                "Seller is already verified.");
        }

        seller.SellerVerificationStatus =
            "verified";

        seller.SellerVerificationReason =
            null;

        seller.SellerVerifiedByUserId =
            adminUserId;

        seller.SellerVerifiedAt =
            DateTime.UtcNow;

        seller.UpdatedAt =
            DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return await GetSellerAsync(
            sellerUserId);
    }

    public async Task<AdminSellerResponse>
        RejectAsync(
            Guid adminUserId,
            Guid sellerUserId,
            SellerVerificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(
            request.Reason))
        {
            throw new InvalidOperationException(
                "Rejection reason is required.");
        }

        var seller =
            await _dbContext.Users
                .SingleOrDefaultAsync(x =>
                    x.Id == sellerUserId &&
                    x.Role == "seller");

        if (seller is null)
        {
            throw new KeyNotFoundException(
                "Seller not found.");
        }

        seller.SellerVerificationStatus =
            "rejected";

        seller.SellerVerificationReason =
            request.Reason.Trim();

        seller.SellerVerifiedByUserId =
            adminUserId;

        seller.SellerVerifiedAt =
            DateTime.UtcNow;

        seller.UpdatedAt =
            DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return await GetSellerAsync(
            sellerUserId);
    }
}