using KhojMarket.Api.Data;
using KhojMarket.Api.DTOs;
using KhojMarket.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KhojMarket.Api.Services;

public class SellerInquiryService
{


    private readonly KhojMarketDbContext _dbContext;
    private readonly SellerCreditService _creditService;
    private readonly IBuyerInquiryNotificationService _notificationService;
    private readonly ILogger<SellerInquiryService> _logger;

    public SellerInquiryService(
        KhojMarketDbContext dbContext,
        SellerCreditService creditService,
        IBuyerInquiryNotificationService notificationService,
        ILogger<SellerInquiryService> logger)
    {
        _dbContext = dbContext;
        _creditService = creditService;
        _notificationService = notificationService;
        _logger = logger;
    }
    public async Task<SellerInquiryResponse> SendAsync(
        Guid sellerUserId,
        Guid requirementId,
        CreateSellerInquiryRequest request)
    {
        var seller = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(x =>
                x.Id == sellerUserId);

        if (seller is null)
        {
            throw new KeyNotFoundException(
                "Seller not found.");
        }

        if (seller.Role != "seller")
        {
            throw new UnauthorizedAccessException(
                "Only sellers can send inquiries.");
        }

        if (seller.SellerVerificationStatus != "verified")
        {
            throw new InvalidOperationException(
                "Seller verification is required before sending an inquiry.");
        }

        if (!seller.PhoneVerified)
        {
            throw new InvalidOperationException(
                "Phone verification is required before sending an inquiry.");
        }

        var requirement =
            await _dbContext.Requirements
                .AsNoTracking()
                .SingleOrDefaultAsync(x =>
                    x.Id == requirementId &&
                    !x.IsDeleted);

        if (requirement is null)
        {
            throw new KeyNotFoundException(
                "Requirement not found.");
        }

        if (requirement.Status != "verified" ||
            requirement.ActivityStatus != "active")
        {
            throw new InvalidOperationException(
                "This requirement is not currently accepting inquiries.");
        }

        if (requirement.UserId == sellerUserId)
        {
            throw new InvalidOperationException(
                "You cannot send an inquiry to your own requirement.");
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new InvalidOperationException(
                "Inquiry message is required.");
        }

        if (request.OfferedPrice.HasValue &&
            request.OfferedPrice.Value < 0)
        {
            throw new InvalidOperationException(
                "Offered price cannot be negative.");
        }

        var alreadyExists =
            await _dbContext.SellerInquiries
                .AnyAsync(x =>
                    x.RequirementId == requirementId &&
                    x.SellerUserId == sellerUserId);

        if (alreadyExists)
        {
            throw new InvalidOperationException(
                "You have already sent an inquiry for this requirement.");
        }

        var now = DateTime.UtcNow;

        var inquiry =
            new SellerInquiry
            {
                Id = Guid.NewGuid(),

                RequirementId =
                    requirement.Id,

                BuyerUserId =
                    requirement.UserId,

                SellerUserId =
                    sellerUserId,

                Message =
                    request.Message.Trim(),

                OfferedPrice =
                    request.OfferedPrice,

                Status = "sent",

                CreatedAt = now,

                UpdatedAt = now
            };

        await using var transaction =
            await _dbContext.Database
                .BeginTransactionAsync();

        try
        {
            _dbContext.SellerInquiries.Add(
                inquiry);

            await _creditService
                .DeductInquiryCreditsAsync(
                    sellerUserId,
                    inquiry.Id);

            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        var response = await GetResponseAsync(
            inquiry.Id);

        // Notification is intentionally AFTER the DB transaction commit.
        // A notification-provider failure must never roll back a valid inquiry
        // or restore already-deducted seller credits.
        try
        {
            var buyer = await _dbContext.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(x =>
                    x.Id == inquiry.BuyerUserId);

            if (buyer is not null)
            {
                await _notificationService.NotifyNewInquiryAsync(
                    new BuyerInquiryNotification
                    {
                        BuyerUserId = buyer.Id,
                        BuyerName = buyer.Name,
                        BuyerEmail = buyer.Email,
                        BuyerPhone = buyer.Phone,
                        SellerName = seller.Name,
                        RequirementReferenceNo = response.RequirementReferenceNo,
                        RequirementTitle = response.RequirementTitle,
                        Message = response.Message,
                        OfferedPrice = response.OfferedPrice
                    });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Inquiry {InquiryId} was saved successfully, but buyer notification failed.",
                inquiry.Id);
        }

        return response;
    }

    public async Task<List<SellerInquiryResponse>>
        GetForBuyerRequirementAsync(
            Guid buyerUserId,
            Guid requirementId)
    {
        var requirement =
            await _dbContext.Requirements
                .AsNoTracking()
                .SingleOrDefaultAsync(x =>
                    x.Id == requirementId &&
                    !x.IsDeleted);

        if (requirement is null)
        {
            throw new KeyNotFoundException(
                "Requirement not found.");
        }

        if (requirement.UserId != buyerUserId)
        {
            throw new UnauthorizedAccessException(
                "You cannot view inquiries for this requirement.");
        }

        return await _dbContext.SellerInquiries
            .AsNoTracking()
            .Where(x =>
                x.RequirementId == requirementId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x =>
                new SellerInquiryResponse
                {
                    Id = x.Id,

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

                    SellerPhone =
                        x.SellerUser.Phone,

                    SellerEmail =
                        x.SellerUser.Email,

                    Message =
                        x.Message,

                    OfferedPrice =
                        x.OfferedPrice,

                    Status =
                        x.Status,

                    CreatedAt =
                        x.CreatedAt,

                    ViewedAt =
                        x.ViewedAt,

                    RespondedAt =
                        x.RespondedAt
                })
            .ToListAsync();
    }

    public async Task<List<SellerInquiryResponse>>
        GetSellerMineAsync(
            Guid sellerUserId)
    {
        return await _dbContext.SellerInquiries
            .AsNoTracking()
            .Where(x =>
                x.SellerUserId == sellerUserId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x =>
                new SellerInquiryResponse
                {
                    Id = x.Id,

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

                    SellerPhone =
                        x.SellerUser.Phone,

                    SellerEmail =
                        x.SellerUser.Email,

                    Message =
                        x.Message,

                    OfferedPrice =
                        x.OfferedPrice,

                    Status =
                        x.Status,

                    CreatedAt =
                        x.CreatedAt,

                    ViewedAt =
                        x.ViewedAt,

                    RespondedAt =
                        x.RespondedAt
                })
            .ToListAsync();
    }


    public async Task<List<SellerInquiryResponse>>
        GetAllForAdminAsync(
            string? search = null,
            string? status = null)
    {
        var query = _dbContext.SellerInquiries
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) &&
            !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            var cleanStatus = status.Trim().ToLower();
            query = query.Where(x =>
                x.Status.ToLower() == cleanStatus);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            query = query.Where(x =>
                x.Requirement.ReferenceNo.Contains(term) ||
                x.Requirement.Title.Contains(term) ||
                x.SellerUser.Name.Contains(term) ||
                x.SellerUser.Email.Contains(term) ||
                (x.SellerUser.Phone != null &&
                 x.SellerUser.Phone.Contains(term)) ||
                x.BuyerUser.Name.Contains(term) ||
                x.BuyerUser.Email.Contains(term));
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new SellerInquiryResponse
            {
                Id = x.Id,
                RequirementId = x.RequirementId,
                RequirementReferenceNo = x.Requirement.ReferenceNo,
                RequirementTitle = x.Requirement.Title,
                BuyerUserId = x.BuyerUserId,
                SellerUserId = x.SellerUserId,
                SellerName = x.SellerUser.Name,
                SellerPhone = x.SellerUser.Phone,
                SellerEmail = x.SellerUser.Email,
                Message = x.Message,
                OfferedPrice = x.OfferedPrice,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                ViewedAt = x.ViewedAt,
                RespondedAt = x.RespondedAt
            })
            .ToListAsync();
    }

    public async Task<SellerInquiryResponse> MarkViewedAsync(
        Guid buyerUserId,
        Guid inquiryId)
    {
        var inquiry =
            await GetBuyerOwnedInquiryAsync(
                buyerUserId,
                inquiryId);

        if (inquiry.Status == "sent")
        {
            inquiry.Status = "viewed";
            inquiry.ViewedAt = DateTime.UtcNow;
            inquiry.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        return await GetResponseAsync(
            inquiry.Id);
    }

    public async Task<SellerInquiryResponse> AcceptAsync(
        Guid buyerUserId,
        Guid inquiryId)
    {
        var inquiry =
            await GetBuyerOwnedInquiryAsync(
                buyerUserId,
                inquiryId);

        if (inquiry.Requirement.Status == "completed")
        {
            throw new InvalidOperationException(
                "Requirement is already completed.");
        }

        if (inquiry.Status == "rejected")
        {
            throw new InvalidOperationException(
                "Rejected inquiry cannot be accepted.");
        }

        if (inquiry.Status == "accepted")
        {
            return await GetResponseAsync(
                inquiry.Id);
        }

        inquiry.Status = "accepted";
        inquiry.RespondedAt = DateTime.UtcNow;
        inquiry.UpdatedAt = DateTime.UtcNow;

        if (!inquiry.ViewedAt.HasValue)
        {
            inquiry.ViewedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();

        return await GetResponseAsync(
            inquiry.Id);
    }

    public async Task<SellerInquiryResponse> RejectAsync(
        Guid buyerUserId,
        Guid inquiryId)
    {
        var inquiry =
            await GetBuyerOwnedInquiryAsync(
                buyerUserId,
                inquiryId);

        if (inquiry.Status == "accepted")
        {
            throw new InvalidOperationException(
                "Accepted inquiry cannot be rejected.");
        }

        if (inquiry.Status == "rejected")
        {
            return await GetResponseAsync(
                inquiry.Id);
        }

        inquiry.Status = "rejected";
        inquiry.RespondedAt = DateTime.UtcNow;
        inquiry.UpdatedAt = DateTime.UtcNow;

        if (!inquiry.ViewedAt.HasValue)
        {
            inquiry.ViewedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();

        return await GetResponseAsync(
            inquiry.Id);
    }

    private async Task<SellerInquiry>
        GetBuyerOwnedInquiryAsync(
            Guid buyerUserId,
            Guid inquiryId)
    {
        var inquiry =
            await _dbContext.SellerInquiries
                .Include(x => x.Requirement)
                .SingleOrDefaultAsync(x =>
                    x.Id == inquiryId);

        if (inquiry is null)
        {
            throw new KeyNotFoundException(
                "Inquiry not found.");
        }

        if (inquiry.BuyerUserId != buyerUserId)
        {
            throw new UnauthorizedAccessException(
                "You cannot modify this inquiry.");
        }

        return inquiry;
    }

    private async Task<SellerInquiryResponse>
        GetResponseAsync(
            Guid inquiryId)
    {
        var result =
            await _dbContext.SellerInquiries
                .AsNoTracking()
                .Where(x =>
                    x.Id == inquiryId)
                .Select(x =>
                    new SellerInquiryResponse
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

                        SellerPhone =
                            x.SellerUser.Phone,

                        SellerEmail =
                            x.SellerUser.Email,

                        Message =
                            x.Message,

                        OfferedPrice =
                            x.OfferedPrice,

                        Status =
                            x.Status,

                        CreatedAt =
                            x.CreatedAt,

                        ViewedAt =
                            x.ViewedAt,

                        RespondedAt =
                            x.RespondedAt
                    })
                .SingleOrDefaultAsync();

        if (result is null)
        {
            throw new KeyNotFoundException(
                "Inquiry not found.");
        }

        return result;
    }
}