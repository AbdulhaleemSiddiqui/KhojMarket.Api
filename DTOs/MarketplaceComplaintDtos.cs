using System.ComponentModel.DataAnnotations;

namespace KhojMarket.Api.DTOs;

public class CreateMarketplaceComplaintRequest
{
    public Guid ReportedUserId { get; set; }
    public Guid? DealId { get; set; }
    [Required, MaxLength(50)] public string Category { get; set; } = string.Empty;
    [Required, MaxLength(1500)] public string Details { get; set; } = string.Empty;
}

public class ReviewMarketplaceComplaintRequest
{
    [Required, MaxLength(1000)] public string AdminNote { get; set; } = string.Empty;
}

public class MarketplaceComplaintResponse
{
    public Guid Id { get; set; }
    public Guid ReporterUserId { get; set; }
    public string? ReporterName { get; set; }
    public Guid ReportedUserId { get; set; }
    public string? ReportedUserName { get; set; }
    public string? ReportedUserRole { get; set; }
    public Guid? DealId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? AdminNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

public class BlockedUserResponse
{
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public string Role { get; set; } = string.Empty;
    public int WarningCount { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime? BlockedAt { get; set; }
    public string? BlockReason { get; set; }
}
