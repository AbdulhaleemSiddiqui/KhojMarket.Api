using KhojMarket.Api.Data;
using KhojMarket.Api.DTOs;
using KhojMarket.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KhojMarket.Api.Services;

public class MarketplaceReviewService
{
    private readonly KhojMarketDbContext _dbContext;

    public MarketplaceReviewService(
        KhojMarketDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MarketplaceReviewResponse> CreateAsync(
        Guid currentUserId,
        Guid dealId,
        CreateMarketplaceReviewRequest request)
    {
        if (request.Rating < 1 || request.Rating > 5)
        {
            throw new InvalidOperationException(
                "Rating must be between 1 and 5.");
        }

        var deal = await _dbContext.MarketplaceDeals
            .AsNoTracking()
            .SingleOrDefaultAsync(x =>
                x.Id == dealId);

        if (deal is null)
        {
            throw new KeyNotFoundException(
                "Deal not found.");
        }

        if (deal.Status != "completed")
        {
            throw new InvalidOperationException(
                "Reviews are only allowed after a completed deal.");
        }

        Guid revieweeUserId;

        if (currentUserId == deal.BuyerUserId)
        {
            revieweeUserId =
                deal.SellerUserId;
        }
        else if (currentUserId == deal.SellerUserId)
        {
            revieweeUserId =
                deal.BuyerUserId;
        }
        else
        {
            throw new UnauthorizedAccessException(
                "You are not part of this deal.");
        }

        var alreadyReviewed =
            await _dbContext.MarketplaceReviews
                .AnyAsync(x =>
                    x.DealId == dealId &&
                    x.ReviewerUserId == currentUserId);

        if (alreadyReviewed)
        {
            throw new InvalidOperationException(
                "You have already reviewed this deal.");
        }

        var review =
            new MarketplaceReview
            {
                Id = Guid.NewGuid(),

                DealId = dealId,

                ReviewerUserId =
                    currentUserId,

                RevieweeUserId =
                    revieweeUserId,

                Rating =
                    request.Rating,

                Comment =
                    string.IsNullOrWhiteSpace(
                        request.Comment)
                        ? null
                        : request.Comment.Trim(),

                CreatedAt =
                    DateTime.UtcNow
            };

        _dbContext.MarketplaceReviews.Add(
            review);

        await _dbContext.SaveChangesAsync();

        return await GetReviewAsync(
            review.Id);
    }

    public async Task<UserRatingResponse> GetUserRatingAsync(
        Guid userId)
    {
        var user =
            await _dbContext.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(x =>
                    x.Id == userId);

        if (user is null)
        {
            throw new KeyNotFoundException(
                "User not found.");
        }

        var ratings =
            _dbContext.MarketplaceReviews
                .AsNoTracking()
                .Where(x =>
                    x.RevieweeUserId == userId);

        var count =
            await ratings.CountAsync();

        var average =
            count == 0
                ? 0
                : await ratings.AverageAsync(
                    x => x.Rating);

        return new UserRatingResponse
        {
            UserId =
                user.Id,

            UserName =
                user.Name,

            AverageRating =
                Math.Round(
                    average,
                    1),

            ReviewCount =
                count
        };
    }

    public async Task<List<MarketplaceReviewResponse>>
        GetUserReviewsAsync(
            Guid userId)
    {
        return await _dbContext.MarketplaceReviews
            .AsNoTracking()
            .Where(x =>
                x.RevieweeUserId == userId)
            .OrderByDescending(x =>
                x.CreatedAt)
            .Select(x =>
                new MarketplaceReviewResponse
                {
                    Id =
                        x.Id,

                    DealId =
                        x.DealId,

                    ReviewerUserId =
                        x.ReviewerUserId,

                    ReviewerName =
                        x.ReviewerUser.Name,

                    RevieweeUserId =
                        x.RevieweeUserId,

                    RevieweeName =
                        x.RevieweeUser.Name,

                    Rating =
                        x.Rating,

                    Comment =
                        x.Comment,

                    CreatedAt =
                        x.CreatedAt
                })
            .ToListAsync();
    }

    public async Task<List<MarketplaceReviewResponse>>
        GetDealReviewsAsync(
            Guid currentUserId,
            Guid dealId)
    {
        var deal =
            await _dbContext.MarketplaceDeals
                .AsNoTracking()
                .SingleOrDefaultAsync(x =>
                    x.Id == dealId);

        if (deal is null)
        {
            throw new KeyNotFoundException(
                "Deal not found.");
        }

        if (currentUserId != deal.BuyerUserId &&
            currentUserId != deal.SellerUserId)
        {
            throw new UnauthorizedAccessException(
                "You are not part of this deal.");
        }

        return await _dbContext.MarketplaceReviews
            .AsNoTracking()
            .Where(x =>
                x.DealId == dealId)
            .OrderBy(x =>
                x.CreatedAt)
            .Select(x =>
                new MarketplaceReviewResponse
                {
                    Id =
                        x.Id,

                    DealId =
                        x.DealId,

                    ReviewerUserId =
                        x.ReviewerUserId,

                    ReviewerName =
                        x.ReviewerUser.Name,

                    RevieweeUserId =
                        x.RevieweeUserId,

                    RevieweeName =
                        x.RevieweeUser.Name,

                    Rating =
                        x.Rating,

                    Comment =
                        x.Comment,

                    CreatedAt =
                        x.CreatedAt
                })
            .ToListAsync();
    }

    private async Task<MarketplaceReviewResponse>
        GetReviewAsync(
            Guid reviewId)
    {
        var result =
            await _dbContext.MarketplaceReviews
                .AsNoTracking()
                .Where(x =>
                    x.Id == reviewId)
                .Select(x =>
                    new MarketplaceReviewResponse
                    {
                        Id =
                            x.Id,

                        DealId =
                            x.DealId,

                        ReviewerUserId =
                            x.ReviewerUserId,

                        ReviewerName =
                            x.ReviewerUser.Name,

                        RevieweeUserId =
                            x.RevieweeUserId,

                        RevieweeName =
                            x.RevieweeUser.Name,

                        Rating =
                            x.Rating,

                        Comment =
                            x.Comment,

                        CreatedAt =
                            x.CreatedAt
                    })
                .SingleOrDefaultAsync();

        if (result is null)
        {
            throw new KeyNotFoundException(
                "Review not found.");
        }

        return result;
    }
}