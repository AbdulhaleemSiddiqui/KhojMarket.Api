namespace KhojMarket.Api.DTOs;

public class SellerInquiryResponse
{
    public Guid Id { get; set; }

    public Guid RequirementId { get; set; }

    public string RequirementReferenceNo { get; set; } = string.Empty;

    public string RequirementTitle { get; set; } = string.Empty;

    public Guid BuyerUserId { get; set; }

    public Guid SellerUserId { get; set; }

    public string SellerName { get; set; } = string.Empty;

    public string SellerPhone { get; set; } = string.Empty;

    public string SellerEmail { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public decimal? OfferedPrice { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? ViewedAt { get; set; }

    public DateTime? RespondedAt { get; set; }
}