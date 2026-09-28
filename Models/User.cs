namespace KhojMarket.Api.Models;

public class User
{
    public string PasswordHash { get; set; } = string.Empty;

    public Guid Id { get; set; } = Guid.NewGuid();

    public string ReferenceNo { get; set; } = string.Empty;

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string Role { get; set; } = "buyer";

    public bool PhoneVerified { get; set; }

    public string? SellerVerificationStatus { get; set; }

    public string? SellerVerificationReason { get; set; }

    public Guid? SellerVerifiedByUserId { get; set; }

    public DateTime? SellerVerifiedAt { get; set; }

    public bool IsBlocked { get; set; }

    public DateTime? BlockedAt { get; set; }

    public string? BlockReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Requirement> Requirements { get; set; }
        = new List<Requirement>();
}