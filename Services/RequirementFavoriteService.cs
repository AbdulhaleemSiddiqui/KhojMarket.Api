using KhojMarket.Api.Data;
using KhojMarket.Api.DTOs;
using KhojMarket.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KhojMarket.Api.Services;

public class RequirementFavoriteService
{
    private readonly KhojMarketDbContext _dbContext;

    public RequirementFavoriteService(
        KhojMarketDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FavoriteRequirementResponse> AddAsync(
        Guid userId,
        Guid requirementId)
    {
        var requirementExists =
            await _dbContext.Requirements
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == requirementId &&
                    !x.IsDeleted &&
                    x.Status == "verified" &&
                    x.ActivityStatus == "active");

        if (!requirementExists)
        {
            throw new KeyNotFoundException(
                "Requirement not found or is not currently available.");
        }

        var exists =
            await _dbContext.RequirementFavorites
                .AnyAsync(x =>
                    x.UserId == userId &&
                    x.RequirementId == requirementId);

        if (!exists)
        {
            var favorite =
                new RequirementFavorite
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    RequirementId = requirementId,
                    CreatedAt = DateTime.UtcNow
                };

            _dbContext.RequirementFavorites.Add(
                favorite);

            await _dbContext.SaveChangesAsync();
        }

        return new FavoriteRequirementResponse
        {
            RequirementId = requirementId,
            IsFavorite = true
        };
    }

    public async Task<FavoriteRequirementResponse> RemoveAsync(
        Guid userId,
        Guid requirementId)
    {
        var favorite =
            await _dbContext.RequirementFavorites
                .SingleOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.RequirementId == requirementId);

        if (favorite is not null)
        {
            _dbContext.RequirementFavorites.Remove(
                favorite);

            await _dbContext.SaveChangesAsync();
        }

        return new FavoriteRequirementResponse
        {
            RequirementId = requirementId,
            IsFavorite = false
        };
    }

    public async Task<List<Guid>> GetFavoriteIdsAsync(
        Guid userId)
    {
        return await _dbContext.RequirementFavorites
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                !x.Requirement.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => x.RequirementId)
            .ToListAsync();
    }
}