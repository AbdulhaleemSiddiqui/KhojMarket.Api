namespace KhojMarket.Api.DTOs;

public class MarketplaceReviewResponse
{
    public Guid Id { get; set; }

    public Guid DealId { get; set; }

    public Guid ReviewerUserId { get; set; }

    public string ReviewerName { get; set; } = string.Empty;

    public Guid RevieweeUserId { get; set; }

    public string RevieweeName { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }
}