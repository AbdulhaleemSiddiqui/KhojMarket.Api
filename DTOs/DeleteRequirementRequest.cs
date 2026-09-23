using System.ComponentModel.DataAnnotations;

namespace KhojMarket.Api.DTOs;

public class DeleteRequirementRequest
{
    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}