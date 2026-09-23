namespace KhojMarket.Api.DTOs;

public class CreditTransactionResponse
{
    public Guid Id { get; set; }

    public Guid? InquiryId { get; set; }

    public string Type { get; set; } = string.Empty;

    public int Amount { get; set; }

    public int BalanceAfter { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}