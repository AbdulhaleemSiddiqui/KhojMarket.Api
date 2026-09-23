namespace KhojMarket.Api.DTOs;

public class SellerWalletResponse
{
    public int Balance { get; set; }

    public int InquiryCost { get; set; }

    public bool CanSendInquiry { get; set; }
}