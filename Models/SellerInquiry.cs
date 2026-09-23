namespace KhojMarket.Api.Models;

public class SellerInquiry
{
    public Guid Id { get; set; }

    public Guid RequirementId { get; set; }
    public Requirement Requirement { get; set; } = null!;

    public Guid BuyerUserId { get; set; }
    public User BuyerUser { get; set; } = null!;

    public Guid SellerUserId { get; set; }
    public User SellerUser { get; set; } = null!;

    public string Message { get; set; } = string.Empty;

    public decimal? OfferedPrice { get; set; }

    public string Status { get; set; } = "sent";

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? ViewedAt { get; set; }

    public DateTime? RespondedAt { get; set; }
}