namespace KhojMarket.Api.Models;

public class Requirement
{
    public ICollection<SellerInquiry> Inquiries { get; set; }
    = new List<SellerInquiry>();
    public string? VerificationMethod { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public string? RejectionReason { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();

    public string ReferenceNo { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string? Category { get; set; }

    public string? SubCategory { get; set; }

    public string RequirementType { get; set; } = "product";

    public string Status { get; set; } = "admin_review_required";

    public string ActivityStatus { get; set; } = "active";

    public decimal? BudgetMin { get; set; }

    public decimal? BudgetMax { get; set; }

    public string? Country { get; set; }

    public string? City { get; set; }

    public string? Area { get; set; }

    public string? PostalCode { get; set; }

    public bool PhoneVerified { get; set; }

    public Guid? AdminApprovedBy { get; set; }

    public DateTime? AdminApprovedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<RequirementField> Fields { get; set; }
        = new List<RequirementField>();

    public ICollection<RequirementImage> Images { get; set; }
        = new List<RequirementImage>();

    public string? ActivityReason { get; set; }

    public string? CompletionReason { get; set; }

    public Guid? CompletedSellerUserId { get; set; }

    public DateTime? CompletedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? DeletionReason { get; set; }
}