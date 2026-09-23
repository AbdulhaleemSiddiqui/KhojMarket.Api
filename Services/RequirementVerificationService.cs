using KhojMarket.Api.Data;
using KhojMarket.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace KhojMarket.Api.Services;

public class RequirementVerificationService
{
    private readonly KhojMarketDbContext _dbContext;

    public RequirementVerificationService(
        KhojMarketDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RequirementVerificationResponse> ApproveAsync(
        Guid requirementId,
        string verificationMethod,
        Guid? approvedByUserId = null)
    {
        var requirement = await _dbContext.Requirements
            .SingleOrDefaultAsync(x => x.Id == requirementId);

        if (requirement is null)
        {
            throw new KeyNotFoundException(
                "Requirement not found.");
        }

        if (requirement.Status != "admin_review_required")
        {
            throw new InvalidOperationException(
                $"Requirement cannot be approved from status '{requirement.Status}'.");
        }

        ValidateVerificationMethod(verificationMethod);

        var now = DateTime.UtcNow;

        requirement.Status = "verified";
        requirement.VerificationMethod = verificationMethod;
        requirement.VerifiedAt = now;
        requirement.RejectionReason = null;
        requirement.UpdatedAt = now;

        if (verificationMethod == "admin")
        {
            if (approvedByUserId is null)
            {
                throw new InvalidOperationException(
                    "Admin user is required for admin approval.");
            }

            requirement.AdminApprovedBy =
                approvedByUserId.Value;

            requirement.AdminApprovedAt = now;
        }
        else
        {
            requirement.AdminApprovedBy = null;
            requirement.AdminApprovedAt = null;
        }

        await _dbContext.SaveChangesAsync();

        return Map(requirement);
    }

    public async Task<RequirementVerificationResponse> RejectAsync(
        Guid requirementId,
        string reason,
        string verificationMethod,
        Guid? rejectedByUserId = null)
    {
        var requirement = await _dbContext.Requirements
            .SingleOrDefaultAsync(x => x.Id == requirementId);

        if (requirement is null)
        {
            throw new KeyNotFoundException(
                "Requirement not found.");
        }

        if (requirement.Status != "admin_review_required")
        {
            throw new InvalidOperationException(
                $"Requirement cannot be rejected from status '{requirement.Status}'.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException(
                "Rejection reason is required.");
        }

        ValidateVerificationMethod(verificationMethod);

        var now = DateTime.UtcNow;

        requirement.Status = "rejected";
        requirement.VerificationMethod = verificationMethod;
        requirement.VerifiedAt = null;
        requirement.RejectionReason = reason.Trim();
        requirement.UpdatedAt = now;

        if (verificationMethod == "admin")
        {
            requirement.AdminApprovedBy =
                rejectedByUserId;

            requirement.AdminApprovedAt = now;
        }
        else
        {
            requirement.AdminApprovedBy = null;
            requirement.AdminApprovedAt = null;
        }

        await _dbContext.SaveChangesAsync();

        return Map(requirement);
    }

    private static void ValidateVerificationMethod(
        string verificationMethod)
    {
        if (verificationMethod != "admin" &&
            verificationMethod != "ai")
        {
            throw new InvalidOperationException(
                "Verification method must be 'admin' or 'ai'.");
        }
    }

    private static RequirementVerificationResponse Map(
        Models.Requirement requirement)
    {
        return new RequirementVerificationResponse
        {
            Id = requirement.Id,
            ReferenceNo = requirement.ReferenceNo,
            Status = requirement.Status,
            VerificationMethod =
                requirement.VerificationMethod ?? string.Empty,
            VerifiedAt = requirement.VerifiedAt,
            RejectionReason = requirement.RejectionReason
        };
    }
}