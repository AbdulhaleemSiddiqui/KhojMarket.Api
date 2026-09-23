using System.ComponentModel.DataAnnotations;

namespace KhojMarket.Api.DTOs;

public class CreateMarketplaceReviewRequest
{
    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }
}