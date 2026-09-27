namespace KhojMarket.Api.Models;

public class MarketplaceComplaint
{
    public Guid Id { get; set; }
    public Guid ReporterUserId { get; set; }
    public User ReporterUser { get; set; } = null!;
    public Guid ReportedUserId { get; set; }
    public User ReportedUser { get; set; } = null!;
    public Guid? DealId { get; set; }
    public MarketplaceDeal? Deal { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public string? AdminNote { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
