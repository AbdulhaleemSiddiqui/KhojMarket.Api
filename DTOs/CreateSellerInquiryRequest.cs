using System.ComponentModel.DataAnnotations;

namespace KhojMarket.Api.DTOs;

public class CreateSellerInquiryRequest
{
    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    public decimal? OfferedPrice { get; set; }
}