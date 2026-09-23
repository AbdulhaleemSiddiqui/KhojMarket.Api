using System.ComponentModel.DataAnnotations;

namespace KhojMarket.Api.DTOs;

public class RejectRequirementRequest
{
    [Required]
    [MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;
}