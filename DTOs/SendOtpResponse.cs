namespace KhojMarket.Api.DTOs;

public class SendOtpResponse
{
    public string Message { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    // Development only.
    public string? DevelopmentOtp { get; set; }
}