namespace KhojMarket.Api.Services;

public interface ISmsService
{
    Task SendOtpAsync(
        string phone,
        string otp);
}