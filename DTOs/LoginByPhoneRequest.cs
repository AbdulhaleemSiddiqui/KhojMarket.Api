namespace KhojMarket.Api.DTOs;

public class LoginByPhoneRequest
{
    public string Phone { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
