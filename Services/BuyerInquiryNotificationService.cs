namespace KhojMarket.Api.Services;

public sealed class BuyerInquiryNotificationService
    : IBuyerInquiryNotificationService
{
    private readonly ILogger<BuyerInquiryNotificationService> _logger;

    public BuyerInquiryNotificationService(
        ILogger<BuyerInquiryNotificationService> logger)
    {
        _logger = logger;
    }

    public Task NotifyNewInquiryAsync(
        BuyerInquiryNotification notification,
        CancellationToken cancellationToken = default)
    {
        // Development implementation.
        // Actual Email + SMS provider next step mein yahan connect hoga.
        _logger.LogInformation(
            "New inquiry notification. Buyer={BuyerUserId}, Email={Email}, Phone={Phone}, Requirement={RequirementReferenceNo}",
            notification.BuyerUserId,
            notification.BuyerEmail,
            notification.BuyerPhone,
            notification.RequirementReferenceNo);

        return Task.CompletedTask;
    }
}