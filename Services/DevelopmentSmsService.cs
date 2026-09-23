namespace KhojMarket.Api.Services;

public class DevelopmentSmsService : ISmsService
{
    private readonly ILogger<DevelopmentSmsService> _logger;

    public DevelopmentSmsService(
        ILogger<DevelopmentSmsService> logger)
    {
        _logger = logger;
    }

    public Task SendOtpAsync(
        string phone,
        string otp)
    {
        _logger.LogInformation(
            "Development OTP for {Phone}: {Otp}",
            phone,
            otp);

        return Task.CompletedTask;
    }
}