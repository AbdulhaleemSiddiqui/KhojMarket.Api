namespace KhojMarket.Api.DTOs;

public class AdminSellerResponse
{
    public Guid Id { get; set; }

    public string ReferenceNo { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public bool PhoneVerified { get; set; }

    public string SellerVerificationStatus { get; set; } = string.Empty;

    public string? SellerVerificationReason { get; set; }

    public DateTime? SellerVerifiedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}