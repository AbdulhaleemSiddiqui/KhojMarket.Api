namespace KhojMarket.Api.Models;

public class MarketplaceDeal
{
    public Guid Id { get; set; }

    public Guid RequirementId { get; set; }
    public Requirement Requirement { get; set; } = null!;

    public Guid BuyerUserId { get; set; }
    public User BuyerUser { get; set; } = null!;

    public Guid SellerUserId { get; set; }
    public User SellerUser { get; set; } = null!;

    public Guid InquiryId { get; set; }
    public SellerInquiry Inquiry { get; set; } = null!;

    public string Status { get; set; } = "completed";

    public DateTime CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}