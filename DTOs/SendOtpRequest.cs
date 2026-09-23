using System.ComponentModel.DataAnnotations;

namespace KhojMarket.Api.DTOs;

public class SendOtpRequest
{
    [Required]
    public string Phone { get; set; } = string.Empty;
}