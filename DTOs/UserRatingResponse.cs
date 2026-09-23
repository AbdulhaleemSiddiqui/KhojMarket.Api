namespace KhojMarket.Api.DTOs;

public class UserRatingResponse
{
    public Guid UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public double AverageRating { get; set; }

    public int ReviewCount { get; set; }
}