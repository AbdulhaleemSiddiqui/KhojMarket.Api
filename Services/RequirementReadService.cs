using KhojMarket.Api.Data;
using KhojMarket.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace KhojMarket.Api.Services;

public class RequirementReadService
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;

    private readonly KhojMarketDbContext _dbContext;

    public RequirementReadService(
        KhojMarketDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResponse<RequirementListItemResponse>>
        GetPendingForAdminAsync(
            RequirementListQueryRequest request)
    {
        var query = _dbContext.Requirements
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.Status == "admin_review_required");

        query = ApplyCommonFilters(
            query,
            request);

        return await CreatePagedResponseAsync(
            query,
            request);
    }

    public async Task<PagedResponse<RequirementListItemResponse>>
       GetPublicAsync(
           RequirementListQueryRequest request)
    {
        var query = _dbContext.Requirements
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.Status == "verified" &&
                x.ActivityStatus == "active");

        query = ApplyCommonFilters(
            query,
            request);

        return await CreatePagedResponseAsync(
            query,
            request);
    }

    public async Task<PagedResponse<RequirementListItemResponse>>
        GetMineAsync(
            Guid userId,
            RequirementListQueryRequest request)
    {
        var query = _dbContext.Requirements
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.UserId == userId);

        query = ApplyCommonFilters(
            query,
            request);

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status =
                request.Status.Trim().ToLower();

            query = query.Where(
                x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(
            request.ActivityStatus))
        {
            var activityStatus =
                request.ActivityStatus
                    .Trim()
                    .ToLower();

            query = query.Where(
                x => x.ActivityStatus ==
                     activityStatus);
        }

        return await CreatePagedResponseAsync(
            query,
            request);
    }
    private static IQueryable<Models.Requirement>
        ApplyCommonFilters(
            IQueryable<Models.Requirement> query,
            RequirementListQueryRequest request)
    {
        if (!string.IsNullOrWhiteSpace(
            request.Search))
        {
            var search =
                request.Search.Trim();

            query = query.Where(x =>
                x.Title.Contains(search) ||
                x.ReferenceNo.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(
            request.Category))
        {
            var category =
                request.Category.Trim();

            query = query.Where(x =>
                x.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(
            request.RequirementType))
        {
            var requirementType =
                request.RequirementType
                    .Trim()
                    .ToLower();

            query = query.Where(x =>
                x.RequirementType ==
                requirementType);
        }

        if (!string.IsNullOrWhiteSpace(
            request.Country))
        {
            var country =
                request.Country.Trim();

            query = query.Where(x =>
                x.Country == country);
        }

        if (!string.IsNullOrWhiteSpace(
            request.City))
        {
            var city =
                request.City.Trim();

            query = query.Where(x =>
                x.City == city);
        }

        if (!string.IsNullOrWhiteSpace(
            request.Area))
        {
            var area =
                request.Area.Trim();

            query = query.Where(x =>
                x.Area != null &&
                x.Area.Contains(area));
        }

        if (!string.IsNullOrWhiteSpace(
            request.PostalCode))
        {
            var postalCode =
                request.PostalCode.Trim();

            query = query.Where(x =>
                x.PostalCode != null &&
                x.PostalCode == postalCode);
        }

        // Range-overlap logic:
        // requirement max must reach selected min.
        if (request.BudgetMin.HasValue)
        {
            var budgetMin =
                request.BudgetMin.Value;

            query = query.Where(x =>
                x.BudgetMax == null ||
                x.BudgetMax >= budgetMin);
        }

        // requirement min must not exceed selected max.
        if (request.BudgetMax.HasValue)
        {
            var budgetMax =
                request.BudgetMax.Value;

            query = query.Where(x =>
                x.BudgetMin == null ||
                x.BudgetMin <= budgetMax);
        }

        if (!string.IsNullOrWhiteSpace(
            request.PostedDate))
        {
            var postedDate =
                request.PostedDate
                    .Trim()
                    .ToLower();

            var utcNow =
                DateTime.UtcNow;

            DateTime? fromDate =
                postedDate switch
                {
                    "today" =>
                        utcNow.Date,

                    "7days" or "last7days" =>
                        utcNow.AddDays(-7),

                    "30days" or "last30days" =>
                        utcNow.AddDays(-30),

                    _ => null
                };

            if (fromDate.HasValue)
            {
                var date =
                    fromDate.Value;

                query = query.Where(x =>
                    x.CreatedAt >= date);
            }
        }

        return query;
    }

    private static async Task<
        PagedResponse<RequirementListItemResponse>>
        CreatePagedResponseAsync(
            IQueryable<Models.Requirement> query,
            RequirementListQueryRequest request)
    {
        var page =
            request.Page < 1
                ? 1
                : request.Page;

        var pageSize =
            request.PageSize <= 0
                ? DefaultPageSize
                : Math.Min(
                    request.PageSize,
                    MaxPageSize);

        var totalCount =
            await query.CountAsync();

        var totalPages =
            totalCount == 0
                ? 0
                : (int)Math.Ceiling(
                    totalCount /
                    (double)pageSize);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x =>
                new RequirementListItemResponse
                {
                    Id = x.Id,

                    ReferenceNo =
                        x.ReferenceNo,

                    Title =
                        x.Title,

                    Category =
                        x.Category,

                    SubCategory =
                        x.SubCategory,

                    RequirementType =
                        x.RequirementType,

                    Status =
                        x.Status,

                    ActivityStatus =
                        x.ActivityStatus,

                    BudgetMin =
                        x.BudgetMin,

                    BudgetMax =
                        x.BudgetMax,

                    Country =
                        x.Country,

                    City =
                        x.City,

                    Area =
                        x.Area,

                    CoverImagePath =
                        x.Images
                            .OrderByDescending(
                                image =>
                                    image.IsCover)
                            .ThenBy(
                                image =>
                                    image.SortOrder)
                            .Select(
                                image =>
                                    image.FilePath)
                            .FirstOrDefault(),
                    InquiryCount =
                        x.Inquiries.Count(),

                    SummaryFields =
                        x.Fields
                            .Where(field =>
                                field.Value != null &&
                                field.Value != "")
                            .OrderBy(field =>
                                field.FieldKey == "quantity" ||
                                field.FieldKey == "qty"
                                    ? 0
                                    : 1)
                            .ThenBy(field =>
                                field.SortOrder)
                            .Take(1)
                            .Select(field =>
                                new RequirementSummaryFieldResponse
                                {
                                    FieldKey =
                                        field.FieldKey,

                                    Label =
                                        field.Label,

                                    Value =
                                        field.Value
                                })
                            .ToList(),

                    CreatedAt =
                        x.CreatedAt
                })
            .ToListAsync();

        return new PagedResponse<
            RequirementListItemResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}