using KhojMarket.Api.Data;
using KhojMarket.Api.DTOs;
using KhojMarket.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KhojMarket.Api.Services;

public class RequirementLifecycleService
{
    private readonly KhojMarketDbContext _dbContext;

    public RequirementLifecycleService(
        KhojMarketDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RequirementResponse> GetDetailsAsync(
        Guid requirementId,
        Guid? currentUserId = null)
    {
        var requirement = await _dbContext.Requirements
            .AsNoTracking()
            .Include(x => x.Fields)
            .Include(x => x.Images)
            .SingleOrDefaultAsync(x =>
                x.Id == requirementId &&
                !x.IsDeleted);

        if (requirement is null)
        {
            throw new KeyNotFoundException(
                "Requirement not found.");
        }

        var isOwner =
            currentUserId.HasValue &&
            requirement.UserId == currentUserId.Value;

        var isPublic =
            requirement.Status == "verified" &&
            requirement.ActivityStatus == "active";

        if (!isOwner && !isPublic)
        {
            throw new UnauthorizedAccessException(
                "You cannot view this requirement.");
        }

        return MapResponse(requirement);
    }

    public async Task<RequirementResponse> UpdateActivityAsync(
        Guid userId,
        Guid requirementId,
        UpdateRequirementActivityRequest request)
    {
        var requirement = await GetOwnedAsync(
            userId,
            requirementId);

        if (requirement.Status == "completed")
        {
            throw new InvalidOperationException(
                "Completed requirement cannot be activated or deactivated.");
        }

        var activityStatus =
            request.ActivityStatus.Trim().ToLowerInvariant();

        if (activityStatus != "active" &&
            activityStatus != "inactive")
        {
            throw new InvalidOperationException(
                "ActivityStatus must be 'active' or 'inactive'.");
        }

        if (activityStatus == "inactive" &&
            string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new InvalidOperationException(
                "Reason is required when making requirement inactive.");
        }

        requirement.ActivityStatus = activityStatus;

        requirement.ActivityReason =
            activityStatus == "inactive"
                ? request.Reason?.Trim()
                : null;

        requirement.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapResponse(requirement);
    }

    //public async Task<RequirementResponse> CompleteAsync(
    //    Guid userId,
    //    Guid requirementId,
    //    CompleteRequirementRequest request)
    //{
    //    var requirement = await GetOwnedAsync(
    //        userId,
    //        requirementId);

    //    if (requirement.Status == "completed")
    //    {
    //        throw new InvalidOperationException(
    //            "Requirement is already completed.");
    //    }

    //    if (string.IsNullOrWhiteSpace(request.Reason))
    //    {
    //        throw new InvalidOperationException(
    //            "Completion reason is required.");
    //    }

    //    requirement.Status = "completed";
    //    requirement.ActivityStatus = "inactive";
    //    requirement.CompletionReason = request.Reason.Trim();
    //    requirement.CompletedSellerUserId = request.SellerUserId;
    //    requirement.CompletedAt = DateTime.UtcNow;
    //    requirement.ActivityReason = "completed";
    //    requirement.UpdatedAt = DateTime.UtcNow;

    //    await _dbContext.SaveChangesAsync();

    //    return MapResponse(requirement);
    //}

    public async Task DeleteAsync(
        Guid userId,
        Guid requirementId,
        DeleteRequirementRequest request)
    {
        var requirement = await GetOwnedAsync(
            userId,
            requirementId);

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new InvalidOperationException(
                "Deletion reason is required.");
        }

        requirement.IsDeleted = true;
        requirement.DeletedAt = DateTime.UtcNow;
        requirement.DeletionReason = request.Reason.Trim();
        requirement.ActivityStatus = "inactive";
        requirement.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task<RequirementResponse> UpdateAsync(
        Guid userId,
        Guid requirementId,
        UpdateRequirementRequest request)
    {
        var requirement = await _dbContext.Requirements
            .Include(x => x.Fields)
            .Include(x => x.Images)
            .SingleOrDefaultAsync(x =>
                x.Id == requirementId &&
                !x.IsDeleted);

        if (requirement is null)
        {
            throw new KeyNotFoundException(
                "Requirement not found.");
        }

        if (requirement.UserId != userId)
        {
            throw new UnauthorizedAccessException(
                "You cannot edit this requirement.");
        }

        if (requirement.Status == "completed")
        {
            throw new InvalidOperationException(
                "Completed requirement cannot be edited.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new InvalidOperationException(
                "Title is required.");
        }

        var requirementType =
            request.RequirementType.Trim().ToLowerInvariant();

        if (requirementType != "product" &&
            requirementType != "service")
        {
            throw new InvalidOperationException(
                "RequirementType must be 'product' or 'service'.");
        }

        if (request.BudgetMin.HasValue &&
            request.BudgetMax.HasValue &&
            request.BudgetMin.Value > request.BudgetMax.Value)
        {
            throw new InvalidOperationException(
                "BudgetMin cannot be greater than BudgetMax.");
        }

        requirement.Title = request.Title.Trim();
        requirement.Category = request.Category?.Trim();
        requirement.SubCategory = request.SubCategory?.Trim();
        requirement.RequirementType = requirementType;
        requirement.BudgetMin = request.BudgetMin;
        requirement.BudgetMax = request.BudgetMax;
        requirement.Country = request.Country?.Trim();
        requirement.City = request.City?.Trim();
        requirement.Area = request.Area?.Trim();
        requirement.PostalCode = request.PostalCode?.Trim();

        _dbContext.RequirementFields.RemoveRange(
            requirement.Fields);

        requirement.Fields.Clear();

        var fields =
            request.Fields ??
            new List<CreateRequirementFieldRequest>();

        foreach (var field in fields)
        {
            requirement.Fields.Add(
                new RequirementField
                {
                    Id = Guid.NewGuid(),
                    RequirementId = requirement.Id,
                    FieldKey = field.FieldKey.Trim(),
                    Label = field.Label.Trim(),
                    Value = field.Value.Trim(),
                    IsRequired = field.IsRequired,
                    SortOrder = field.SortOrder
                });
        }

        if (requirement.Status == "verified")
        {
            requirement.Status =
                "admin_review_required";

            requirement.VerificationMethod = null;
            requirement.VerifiedAt = null;
            requirement.AdminApprovedBy = null;
            requirement.AdminApprovedAt = null;
            requirement.RejectionReason = null;
        }
        else if (requirement.Status == "rejected")
        {
            requirement.Status =
                "admin_review_required";

            requirement.VerificationMethod = null;
            requirement.VerifiedAt = null;
            requirement.RejectionReason = null;
            requirement.AdminApprovedBy = null;
            requirement.AdminApprovedAt = null;
        }

        requirement.UpdatedAt =
            DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapResponse(requirement);
    }

    private async Task<Requirement> GetOwnedAsync(
        Guid userId,
        Guid requirementId)
    {
        var requirement = await _dbContext.Requirements
            .Include(x => x.Fields)
            .Include(x => x.Images)
            .SingleOrDefaultAsync(x =>
                x.Id == requirementId &&
                !x.IsDeleted);

        if (requirement is null)
        {
            throw new KeyNotFoundException(
                "Requirement not found.");
        }

        if (requirement.UserId != userId)
        {
            throw new UnauthorizedAccessException(
                "You cannot modify this requirement.");
        }

        return requirement;
    }

    private static RequirementResponse MapResponse(
        Requirement requirement)
    {
        return new RequirementResponse
        {
            Id = requirement.Id,
            ReferenceNo = requirement.ReferenceNo,
            Title = requirement.Title,
            Category = requirement.Category,
            SubCategory = requirement.SubCategory,
            RequirementType = requirement.RequirementType,
            Status = requirement.Status,
            ActivityStatus = requirement.ActivityStatus,
            BudgetMin = requirement.BudgetMin,
            BudgetMax = requirement.BudgetMax,
            Country = requirement.Country,
            City = requirement.City,
            Area = requirement.Area,
            PostalCode = requirement.PostalCode,
            PhoneVerified = requirement.PhoneVerified,
            CreatedAt = requirement.CreatedAt,

            Fields = requirement.Fields
                .OrderBy(x => x.SortOrder)
                .Select(x =>
                    new RequirementFieldResponse
                    {
                        Id = x.Id,
                        FieldKey = x.FieldKey,
                        Label = x.Label,
                        Value = x.Value,
                        IsRequired = x.IsRequired,
                        SortOrder = x.SortOrder
                    })
                .ToList(),

            Images = requirement.Images
                .OrderBy(x => x.SortOrder)
                .Select(x =>
                    new RequirementImageResponse
                    {
                        Id = x.Id,
                        FilePath = x.FilePath,
                        IsCover = x.IsCover,
                        SortOrder = x.SortOrder
                    })
                .ToList()
        };
    }
}