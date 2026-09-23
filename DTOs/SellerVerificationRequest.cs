using System.ComponentModel.DataAnnotations;

namespace KhojMarket.Api.DTOs;

public class SellerVerificationRequest
{
    [MaxLength(500)]
    public string? Reason { get; set; }
}