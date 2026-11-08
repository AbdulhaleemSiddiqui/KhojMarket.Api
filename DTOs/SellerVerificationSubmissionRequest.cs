using System.ComponentModel.DataAnnotations;

namespace KhojMarket.Api.DTOs;

public class SellerVerificationSubmissionRequest
{
    [Required]
    [MaxLength(200)]
    public string BusinessName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;
}
