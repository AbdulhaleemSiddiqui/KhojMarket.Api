namespace KhojMarket.Api.Models;

public class RequirementFavorite
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid RequirementId { get; set; }
    public Requirement Requirement { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}