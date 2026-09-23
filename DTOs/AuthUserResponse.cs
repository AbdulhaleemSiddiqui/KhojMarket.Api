namespace KhojMarket.Api.DTOs;

public class AuthUserResponse
{
    public Guid Id { get; set; }

    public string ReferenceNo { get; set; } = string.Empty;

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string Role { get; set; } = string.Empty;

    public bool PhoneVerified { get; set; }

    public string? SellerVerificationStatus { get; set; }
}