using System.ComponentModel.DataAnnotations;

namespace KhojMarket.Api.DTOs;

public class CompleteRequirementRequest
{
    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    [Required]
    public string CompletionSource { get; set; } = string.Empty;

    public Guid? InquiryId { get; set; }
}