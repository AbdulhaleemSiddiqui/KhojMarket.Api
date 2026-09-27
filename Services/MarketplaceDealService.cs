using KhojMarket.Api.Data;
using KhojMarket.Api.DTOs;
using KhojMarket.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KhojMarket.Api.Services;

public class MarketplaceDealService
{
    private readonly KhojMarketDbContext _dbContext;

    public MarketplaceDealService(
        KhojMarketDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MarketplaceDealResponse?> CompleteAsync(
        Guid buyerUserId,
        Guid requirementId,
        CompleteRequirementRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new InvalidOperationException(
                "Completion reason is required.");
        }

        var allowedSources = new[]
        {
            "khojmarket_seller",
            "elsewhere",
            "no_longer_needed"
        };

        var completionSource =
            request.CompletionSource
                .Trim()
                .ToLowerInvariant();

        if (!allowedSources.Contains(completionSource))
        {
            throw new InvalidOperationException(
                "Invalid completion source.");
        }

        var requirement =
            await _dbContext.Requirements
                .SingleOrDefaultAsync(x =>
                    x.Id == requirementId &&
                    x.UserId == buyerUserId &&
                    !x.IsDeleted);

        if (requirement is null)
        {
            throw new KeyNotFoundException(
                "Requirement not found.");
        }

        if (requirement.Status == "completed")
        {
            throw new InvalidOperationException(
                "Requirement is already completed.");
        }

        if (completionSource != "khojmarket_seller")
        {
            if (request.InquiryId.HasValue)
            {
                throw new InvalidOperationException(
                    "InquiryId must only be supplied when completing through a KhojMarket seller.");
            }

            requirement.Status =
                "completed";

            requirement.ActivityStatus =
                "inactive";

            requirement.CompletionReason =
                request.Reason.Trim();

            requirement.CompletedSellerUserId =
                null;

            requirement.CompletedAt =
                DateTime.UtcNow;

            requirement.UpdatedAt =
                DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return null;
        }

        if (!request.InquiryId.HasValue)
        {
            throw new InvalidOperationException(
                "Select an accepted seller inquiry.");
        }

        var inquiry =
            await _dbContext.SellerInquiries
                .SingleOrDefaultAsync(x =>
                    x.Id == request.InquiryId.Value &&
                    x.RequirementId == requirementId &&
                    x.BuyerUserId == buyerUserId);

        if (inquiry is null)
        {
            throw new KeyNotFoundException(
                "Seller inquiry not found.");
        }

        if (inquiry.Status != "accepted")
        {
            throw new InvalidOperationException(
                "Only an accepted inquiry can be selected for a completed deal.");
        }

        var existingDeal =
            await _dbContext.MarketplaceDeals
                .AnyAsync(x =>
                    x.RequirementId == requirementId);

        if (existingDeal)
        {
            throw new InvalidOperationException(
                "A completed deal already exists for this requirement.");
        }

        var now =
            DateTime.UtcNow;

        var deal =
            new MarketplaceDeal
            {
                Id =
                    Guid.NewGuid(),

                RequirementId =
                    requirement.Id,

                BuyerUserId =
                    buyerUserId,

                SellerUserId =
                    inquiry.SellerUserId,

                InquiryId =
                    inquiry.Id,

                Status =
                    "pending_confirmation",

                BuyerConfirmedAt =
                    now,

                CompletedAt =
                    null,

                CreatedAt =
                    now
            };

        await using var transaction =
            await _dbContext.Database
                .BeginTransactionAsync();

        try
        {
            _dbContext.MarketplaceDeals.Add(
                deal);

            // KhojMarket seller deals require both parties to confirm.
            // Keep the requirement active until the seller confirms completion.
            requirement.CompletionReason =
                request.Reason.Trim();

            requirement.UpdatedAt =
                now;

            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return await GetDealAsync(
            deal.Id);
    }


    public async Task<MarketplaceDealResponse> ConfirmSellerCompletionAsync(
        Guid sellerUserId,
        Guid dealId)
    {
        var deal = await _dbContext.MarketplaceDeals
            .Include(x => x.Requirement)
            .SingleOrDefaultAsync(x => x.Id == dealId);

        if (deal is null)
        {
            throw new KeyNotFoundException("Deal not found.");
        }

        if (deal.SellerUserId != sellerUserId)
        {
            throw new UnauthorizedAccessException("You cannot confirm this deal.");
        }

        if (!deal.BuyerConfirmedAt.HasValue)
        {
            throw new InvalidOperationException("Buyer confirmation is required first.");
        }

        if (deal.Status == "completed")
        {
            return await GetDealAsync(deal.Id);
        }

        var now = DateTime.UtcNow;
        deal.SellerConfirmedAt = now;
        deal.CompletedAt = now;
        deal.Status = "completed";

        deal.Requirement.Status = "completed";
        deal.Requirement.ActivityStatus = "inactive";
        deal.Requirement.CompletedSellerUserId = deal.SellerUserId;
        deal.Requirement.CompletedAt = now;
        deal.Requirement.UpdatedAt = now;

        await _dbContext.SaveChangesAsync();

        return await GetDealAsync(deal.Id);
    }

    public async Task<MarketplaceDealResponse> CancelAsync(
        Guid currentUserId,
        Guid dealId,
        CancelMarketplaceDealRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new InvalidOperationException("Cancellation reason is required.");

        var reason = request.Reason.Trim();
        if (reason.Length > 1000)
            throw new InvalidOperationException("Cancellation reason cannot exceed 1000 characters.");

        var deal = await _dbContext.MarketplaceDeals
            .Include(x => x.Requirement)
            .SingleOrDefaultAsync(x => x.Id == dealId);

        if (deal is null)
            throw new KeyNotFoundException("Deal not found.");

        if (currentUserId != deal.BuyerUserId && currentUserId != deal.SellerUserId)
            throw new UnauthorizedAccessException("You cannot cancel this deal.");

        if (deal.Status == "completed")
            throw new InvalidOperationException("A completed deal cannot be cancelled.");

        if (deal.Status == "cancelled")
            return await GetDealAsync(deal.Id);

        var now = DateTime.UtcNow;
        deal.Status = "cancelled";
        deal.CancelledAt = now;
        deal.CancelledByUserId = currentUserId;
        deal.CancellationReason = reason;

        // A cancelled marketplace deal closes this accepted transaction,
        // but it is not treated as a successful completed deal/review.
        deal.Requirement.ActivityStatus = "inactive";
        deal.Requirement.UpdatedAt = now;

        await _dbContext.SaveChangesAsync();
        return await GetDealAsync(deal.Id);
    }

    public async Task<MarketplaceDealResponse>
        GetDealAsync(
            Guid dealId)
    {
        var result =
            await _dbContext.MarketplaceDeals
                .AsNoTracking()
                .Where(x =>
                    x.Id == dealId)
                .Select(x =>
                    new MarketplaceDealResponse
                    {
                        Id =
                            x.Id,

                        RequirementId =
                            x.RequirementId,

                        RequirementReferenceNo =
                            x.Requirement.ReferenceNo,

                        RequirementTitle =
                            x.Requirement.Title,

                        BuyerUserId =
                            x.BuyerUserId,

                        SellerUserId =
                            x.SellerUserId,

                        SellerName =
                            x.SellerUser.Name,

                        InquiryId =
                            x.InquiryId,

                        Status =
                            x.Status,

                        BuyerConfirmedAt =
                            x.BuyerConfirmedAt,

                        SellerConfirmedAt =
                            x.SellerConfirmedAt,

                        CompletedAt =
                            x.CompletedAt,

                        CancelledAt =
                            x.CancelledAt,

                        CancelledByUserId =
                            x.CancelledByUserId,

                        CancellationReason =
                            x.CancellationReason
                    })
                .SingleOrDefaultAsync();

        if (result is null)
        {
            throw new KeyNotFoundException(
                "Deal not found.");
        }

        return result;
    }

    public async Task<MarketplaceDealResponse?>
        GetRequirementDealAsync(
            Guid requirementId,
            Guid currentUserId)
    {
        return await _dbContext.MarketplaceDeals
            .AsNoTracking()
            .Where(x =>
                x.RequirementId == requirementId &&
                (
                    x.BuyerUserId == currentUserId ||
                    x.SellerUserId == currentUserId
                ))
            .Select(x =>
                new MarketplaceDealResponse
                {
                    Id =
                        x.Id,

                    RequirementId =
                        x.RequirementId,

                    RequirementReferenceNo =
                        x.Requirement.ReferenceNo,

                    RequirementTitle =
                        x.Requirement.Title,

                    BuyerUserId =
                        x.BuyerUserId,

                    SellerUserId =
                        x.SellerUserId,

                    SellerName =
                        x.SellerUser.Name,

                    InquiryId =
                        x.InquiryId,

                    Status =
                            x.Status,

                        BuyerConfirmedAt =
                            x.BuyerConfirmedAt,

                        SellerConfirmedAt =
                            x.SellerConfirmedAt,

                        CompletedAt =
                            x.CompletedAt,

                        CancelledAt =
                            x.CancelledAt,

                        CancelledByUserId =
                            x.CancelledByUserId,

                        CancellationReason =
                            x.CancellationReason
                })
            .SingleOrDefaultAsync();
    }
}