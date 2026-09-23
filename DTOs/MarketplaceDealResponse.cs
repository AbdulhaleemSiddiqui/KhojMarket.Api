namespace KhojMarket.Api.DTOs;

public class MarketplaceDealResponse
{
    public Guid Id { get; set; }

    public Guid RequirementId { get; set; }

    public string RequirementReferenceNo { get; set; } = string.Empty;

    public string RequirementTitle { get; set; } = string.Empty;

    public Guid BuyerUserId { get; set; }

    public Guid SellerUserId { get; set; }

    public string SellerName { get; set; } = string.Empty;

    public Guid InquiryId { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CompletedAt { get; set; }
}