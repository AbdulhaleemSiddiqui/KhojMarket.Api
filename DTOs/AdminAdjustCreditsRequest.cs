using System.ComponentModel.DataAnnotations;

namespace KhojMarket.Api.DTOs;

public class AdminAdjustCreditsRequest
{
    public int Amount { get; set; }

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}