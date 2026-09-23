using System.ComponentModel.DataAnnotations;

namespace KhojMarket.Api.DTOs;

public class VerifyOtpRequest
{
    [Required]
    public string Code { get; set; } = string.Empty;
}