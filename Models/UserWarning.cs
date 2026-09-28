namespace KhojMarket.Api.Models;

public class UserWarning
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid ComplaintId { get; set; }
    public MarketplaceComplaint Complaint { get; set; } = null!;
    public string Reason { get; set; } = string.Empty;
    public Guid IssuedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
