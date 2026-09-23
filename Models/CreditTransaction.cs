namespace KhojMarket.Api.Models;

public class CreditTransaction
{
    public Guid Id { get; set; }

    public Guid SellerUserId { get; set; }

    public Guid? InquiryId { get; set; }

    public string Type { get; set; } = string.Empty;

    public int Amount { get; set; }

    public int BalanceAfter { get; set; }

    public string Description { get; set; } = string.Empty;
    public Guid? PerformedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}