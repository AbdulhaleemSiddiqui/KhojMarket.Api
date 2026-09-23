namespace KhojMarket.Api.DTOs;

public class RequirementVerificationResponse
{
    public Guid Id { get; set; }

    public string ReferenceNo { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string VerificationMethod { get; set; } = string.Empty;

    public DateTime? VerifiedAt { get; set; }

    public string? RejectionReason { get; set; }
}