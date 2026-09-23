namespace KhojMarket.Api.Models;

public class MarketplaceReview
{
    public Guid Id { get; set; }

    public Guid DealId { get; set; }
    public MarketplaceDeal Deal { get; set; } = null!;

    public Guid ReviewerUserId { get; set; }
    public User ReviewerUser { get; set; } = null!;

    public Guid RevieweeUserId { get; set; }
    public User RevieweeUser { get; set; } = null!;

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }
}