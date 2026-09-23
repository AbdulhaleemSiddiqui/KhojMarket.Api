using System.ComponentModel.DataAnnotations;

namespace KhojMarket.Api.DTOs;

public class UpdateRequirementActivityRequest
{
    [Required]
    public string ActivityStatus { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Reason { get; set; }
}