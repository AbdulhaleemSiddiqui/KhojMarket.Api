namespace KhojMarket.Api.Models;

public class SellerWallet
{
    public Guid Id { get; set; }

    public Guid SellerUserId { get; set; }
    public User SellerUser { get; set; } = null!;

    public int Balance { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}