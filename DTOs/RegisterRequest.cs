using System.ComponentModel.DataAnnotations;

namespace KhojMarket.Api.DTOs;

public class RegisterRequest
{
    public string? Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "buyer";
}