namespace KhojMarket.Api.Services;

public sealed class BuyerInquiryNotification
{
    public Guid BuyerUserId { get; init; }
    public string? BuyerName { get; init; }
    public string? BuyerEmail { get; init; }
    public string? BuyerPhone { get; init; }
    public string? SellerName { get; init; }
    public string? RequirementReferenceNo { get; init; }
    public string? RequirementTitle { get; init; }
    public string? Message { get; init; }
    public decimal? OfferedPrice { get; init; }
}

public interface IBuyerInquiryNotificationService
{
    Task NotifyNewInquiryAsync(
        BuyerInquiryNotification notification,
        CancellationToken cancellationToken = default);
}
