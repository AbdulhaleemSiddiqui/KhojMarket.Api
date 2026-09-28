using KhojMarket.Api.Data;
using KhojMarket.Api.DTOs;
using KhojMarket.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KhojMarket.Api.Services;

public class UserService
{
    private readonly KhojMarketDbContext _dbContext;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly JwtService _jwtService;

    public UserService(
        KhojMarketDbContext dbContext,
        JwtService jwtService)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request)
    {
        var email =
            request.Email?
                .Trim()
                .ToLowerInvariant()
            ?? string.Empty;

        var phone =
            NormalizePhone(
                request.Phone);

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException(
                "Email is required.");
        }

        var emailExists =
            await _dbContext.Users
                .AnyAsync(x =>
                    x.Email == email);

        if (emailExists)
        {
            throw new InvalidOperationException(
                "An account with this email already exists.");
        }

        if (!string.IsNullOrWhiteSpace(phone))
        {
            var phoneExists =
                await _dbContext.Users
                    .AnyAsync(x =>
                        x.Phone == phone);

            if (phoneExists)
            {
                throw new InvalidOperationException(
                    "An account with this phone number already exists.");
            }
        }

        var role =
            request.Role
                .Trim()
                .ToLowerInvariant();

        if (role is not ("buyer" or "seller"))
        {
            throw new InvalidOperationException(
                "Role must be buyer or seller.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),

            ReferenceNo =
                GenerateUserReference(),

            Name =
                request.Name?.Trim(),

            Email = email,

            Phone = phone,

            Role = role,

            PhoneVerified = false,

            SellerVerificationStatus =
                role == "seller"
                    ? "pending"
                    : null,

            CreatedAt = DateTime.UtcNow,

            UpdatedAt = DateTime.UtcNow
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(
                user,
                request.Password);

        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync();

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponse?> LoginAsync(
        LoginRequest request)
    {
        var email =
            request.Email
                .Trim()
                .ToLowerInvariant();

        var user =
            await _dbContext.Users
                .SingleOrDefaultAsync(
                    x => x.Email == email);

        return ValidateLogin(
            user,
            request.Password);
    }

    public async Task<AuthResponse> LoginByPhoneAsync(
        LoginByPhoneRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Phone))
        {
            throw new InvalidOperationException(
                "Phone number is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException(
                "Password is required.");
        }

        var phone = NormalizePhone(request.Phone);

        var users = await _dbContext.Users
            .Where(x => x.Phone == phone)
            .Take(2)
            .ToListAsync();

        if (users.Count > 1)
        {
            throw new InvalidOperationException(
                "Multiple accounts are registered with this phone number. Please use email login or contact support.");
        }

        var user = users.SingleOrDefault();

        return ValidateLogin(
            user,
            request.Password);
    }

    public async Task<AuthUserResponse?> GetByIdAsync(
        Guid userId)
    {
        var user =
            await _dbContext.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.Id == userId);

        if (user is null)
        {
            return null;
        }

        return ToUserResponse(user);
    }

    private AuthResponse? ValidateLogin(
        User? user,
        string password)
    {
        if (user is null)
        {
            return null;
        }

        if (user.IsBlocked)
        {
            throw new InvalidOperationException("This account is blocked. Please contact support.");
        }

        var passwordResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                password);

        if (passwordResult ==
            PasswordVerificationResult.Failed)
        {
            return null;
        }

        return CreateAuthResponse(user);
    }

    private AuthResponse CreateAuthResponse(
        User user)
    {
        var tokenData =
            _jwtService.GenerateToken(user);

        return new AuthResponse
        {
            Token = tokenData.Token,

            ExpiresAt =
                tokenData.ExpiresAt,

            User =
                ToUserResponse(user)
        };
    }

    private static AuthUserResponse ToUserResponse(
        User user)
    {
        return new AuthUserResponse
        {
            Id = user.Id,

            ReferenceNo =
                user.ReferenceNo,

            Name = user.Name,

            Email = user.Email,

            Phone = user.Phone,

            Role = user.Role,

            PhoneVerified =
                user.PhoneVerified,

            SellerVerificationStatus =
                user.SellerVerificationStatus
        };
    }

    private static string? NormalizePhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();

        var digits = new string(
            trimmed.Where(char.IsDigit).ToArray());

        if (string.IsNullOrWhiteSpace(digits))
        {
            return null;
        }

        // International format already supplied
        if (trimmed.StartsWith("+"))
        {
            return $"+{digits}";
        }

        // 00 international prefix
        if (trimmed.StartsWith("00"))
        {
            return $"+{digits[2..]}";
        }

        // Pakistan local mobile format: 03XXXXXXXXX
        if (digits.StartsWith("03") && digits.Length == 11)
        {
            return $"+92{digits[1..]}";
        }

        // Pakistan without +
        if (digits.StartsWith("92"))
        {
            return $"+{digits}";
        }

        // Other countries must provide country code
        throw new InvalidOperationException(
            "Please enter the phone number with country code, for example +923001234567.");
    }
    private static string GenerateUserReference()
    {
        var shortId =
            Guid.NewGuid()
                .ToString("N")
                .Substring(0, 8)
                .ToUpperInvariant();

        return $"KM-USR-{shortId}";
    }
}
