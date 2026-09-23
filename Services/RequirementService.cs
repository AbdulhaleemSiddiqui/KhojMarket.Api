using KhojMarket.Api.Data;
using KhojMarket.Api.DTOs;
using KhojMarket.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KhojMarket.Api.Services;

public class RequirementService
{
    private const int MaxImages = 20;
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

    private readonly KhojMarketDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;
    public async Task<RequirementResponse> UploadImagesAsync(
    Guid userId,
    Guid requirementId,
    List<IFormFile> images)
    {
        var requirement = await _dbContext.Requirements
            .AsNoTracking()
            .Include(x => x.Fields)
            .Include(x => x.Images)
            .SingleOrDefaultAsync(x => x.Id == requirementId);

        if (requirement is null)
        {
            throw new KeyNotFoundException(
                "Requirement not found.");
        }

        if (requirement.UserId != userId)
        {
            throw new UnauthorizedAccessException(
                "You cannot upload images to this requirement.");
        }

        if (images is null || images.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one image is required.");
        }

        if (requirement.Images.Count + images.Count > MaxImages)
        {
            throw new InvalidOperationException(
                $"Maximum {MaxImages} images are allowed per requirement.");
        }

        foreach (var image in images)
        {
            ValidateImage(image);
        }

        var savedFiles = new List<string>();

        try
        {
            var existingImageCount =
                requirement.Images.Count;

            var startSortOrder =
                existingImageCount == 0
                    ? 0
                    : requirement.Images.Max(x => x.SortOrder) + 1;

            var newImages =
                new List<RequirementImage>();

            for (var index = 0; index < images.Count; index++)
            {
                var image = images[index];

                var relativePath =
                    await SaveImageAsync(
                        requirement.ReferenceNo,
                        image);

                savedFiles.Add(relativePath);

                var requirementImage =
                    new RequirementImage
                    {
                        Id = Guid.NewGuid(),

                        RequirementId =
                            requirement.Id,

                        FilePath =
                            relativePath,

                        IsCover =
                            existingImageCount == 0 &&
                            index == 0,

                        SortOrder =
                            startSortOrder + index,

                        CreatedAt =
                            DateTime.UtcNow
                    };

                newImages.Add(requirementImage);
            }

            // IMPORTANT:
            // Directly insert only new image rows.
            await _dbContext.RequirementImages
                .AddRangeAsync(newImages);

            await _dbContext.SaveChangesAsync();

            // Response ke liye newly created images add kar rahe hain.
            foreach (var newImage in newImages)
            {
                requirement.Images.Add(newImage);
            }
            return MapResponse(requirement);
        }
        catch
        {
            DeleteFiles(savedFiles);

            throw;
        }
    }
    private static void ValidateImage(IFormFile image)
    {
        var extension =
            Path.GetExtension(image.FileName);

        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                "Only JPG, JPEG, PNG and WEBP images are allowed.");
        }

        if (image.Length <= 0)
        {
            throw new InvalidOperationException(
                "Empty image files are not allowed.");
        }

        if (image.Length > 5 * 1024 * 1024)
        {
            throw new InvalidOperationException(
                "Each image must be 5 MB or smaller.");
        }
    }
    public RequirementService(
        KhojMarketDbContext dbContext,
        IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    public async Task<RequirementResponse> CreateAsync(
        Guid userId,
        CreateRequirementRequest request)
    {
        var user = await _dbContext.Users
            .SingleOrDefaultAsync(x => x.Id == userId);

        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        if (!string.Equals(
                user.Role,
                "buyer",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only buyers can create requirements.");
        }

        if (!user.PhoneVerified)
        {
            throw new InvalidOperationException(
                "Phone verification is required before submitting a requirement.");
        }

        ValidateRequest(request);

        var fields = request.Fields ?? new List<CreateRequirementFieldRequest>();
        var requirement = new Requirement
        {
            Id = Guid.NewGuid(),

            ReferenceNo = GenerateRequirementReference(),

            UserId = user.Id,

            Title = request.Title.Trim(),

            Category = Clean(request.Category),

            SubCategory = Clean(request.SubCategory),

            RequirementType =
                request.RequirementType
                    .Trim()
                    .ToLowerInvariant(),

            // Customer cannot control these values.
            Status = "admin_review_required",

            ActivityStatus = "active",

            BudgetMin = request.BudgetMin,

            BudgetMax = request.BudgetMax,

            Country = Clean(request.Country),

            City = Clean(request.City),

            Area = Clean(request.Area),

            PostalCode = Clean(request.PostalCode),

            PhoneVerified = user.PhoneVerified,

            CreatedAt = DateTime.UtcNow,

            UpdatedAt = DateTime.UtcNow
        };

        foreach (var field in fields)
        {
            requirement.Fields.Add(
                new RequirementField
                {
                    Id = Guid.NewGuid(),

                    FieldKey =
                        field.FieldKey.Trim(),

                    Label =
                        field.Label.Trim(),

                    Value =
                        field.Value.Trim(),

                    IsRequired =
                        field.IsRequired,

                    SortOrder =
                        field.SortOrder
                });
        }

        _dbContext.Requirements.Add(requirement);

        await _dbContext.SaveChangesAsync();

        return MapResponse(requirement);
    }

    private static void ValidateRequest(
        CreateRequirementRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new InvalidOperationException(
                "Requirement title is required.");
        }

        if (request.Title.Trim().Length > 300)
        {
            throw new InvalidOperationException(
                "Requirement title is too long.");
        }

        var requirementType =
            request.RequirementType?
                .Trim()
                .ToLowerInvariant();

        if (requirementType is not ("product" or "service"))
        {
            throw new InvalidOperationException(
                "Requirement type must be product or service.");
        }

        if (request.BudgetMin.HasValue &&
            request.BudgetMin < 0)
        {
            throw new InvalidOperationException(
                "Minimum budget cannot be negative.");
        }

        if (request.BudgetMax.HasValue &&
            request.BudgetMax < 0)
        {
            throw new InvalidOperationException(
                "Maximum budget cannot be negative.");
        }

        if (request.BudgetMin.HasValue &&
            request.BudgetMax.HasValue &&
            request.BudgetMin > request.BudgetMax)
        {
            throw new InvalidOperationException(
                "Minimum budget cannot be greater than maximum budget.");
        }

    }


    private async Task<string> SaveImageAsync(
        string referenceNo,
        IFormFile image)
    {
        var extension =
            Path.GetExtension(image.FileName)
                .ToLowerInvariant();

        var fileName =
            $"{Guid.NewGuid():N}{extension}";

        var webRoot =
            _environment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(
                _environment.ContentRootPath,
                "wwwroot");
        }

        var folder =
            Path.Combine(
                webRoot,
                "uploads",
                "requirements",
                referenceNo);

        Directory.CreateDirectory(folder);

        var physicalPath =
            Path.Combine(folder, fileName);

        await using var stream =
            new FileStream(
                physicalPath,
                FileMode.CreateNew);

        await image.CopyToAsync(stream);

        // Browser/API friendly path.
        return
            $"/uploads/requirements/{referenceNo}/{fileName}";
    }

    private void DeleteFiles(
        IEnumerable<string> relativePaths)
    {
        var webRoot =
            _environment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(
                _environment.ContentRootPath,
                "wwwroot");
        }

        foreach (var relativePath in relativePaths)
        {
            try
            {
                var cleanPath =
                    relativePath
                        .TrimStart('/')
                        .Replace(
                            '/',
                            Path.DirectorySeparatorChar);

                var physicalPath =
                    Path.Combine(
                        webRoot,
                        cleanPath);

                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                }
            }
            catch
            {
                // Do not hide original DB/file exception.
            }
        }
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static string GenerateRequirementReference()
    {
        var shortId =
            Guid.NewGuid()
                .ToString("N")
                .Substring(0, 8)
                .ToUpperInvariant();

        return $"KM-REQ-{shortId}";
    }

    private static RequirementResponse MapResponse(
        Requirement requirement)
    {
        return new RequirementResponse
        {
            Id = requirement.Id,

            ReferenceNo =
                requirement.ReferenceNo,

            Title =
                requirement.Title,

            Category =
                requirement.Category,

            SubCategory =
                requirement.SubCategory,

            RequirementType =
                requirement.RequirementType,

            Status =
                requirement.Status,

            ActivityStatus =
                requirement.ActivityStatus,

            BudgetMin =
                requirement.BudgetMin,

            BudgetMax =
                requirement.BudgetMax,

            Country =
                requirement.Country,

            City =
                requirement.City,

            Area =
                requirement.Area,

            PostalCode =
                requirement.PostalCode,

            PhoneVerified =
                requirement.PhoneVerified,

            CreatedAt =
                requirement.CreatedAt,

            Fields =
                requirement.Fields
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

            Images =
                requirement.Images
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