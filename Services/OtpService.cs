using System.Security.Cryptography;
using System.Text;
using KhojMarket.Api.Data;
using KhojMarket.Api.DTOs;
using KhojMarket.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KhojMarket.Api.Services;

public class OtpService
{
    private const int ExpiryMinutes = 5;
    private const int MaxFailedAttempts = 5;

    private readonly ISmsService _smsService;
    private readonly KhojMarketDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public OtpService(
        KhojMarketDbContext dbContext,
        IWebHostEnvironment environment,
        ISmsService smsService)
    {
        _dbContext = dbContext;
        _environment = environment;
        _smsService = smsService;
    }

    public async Task<SendOtpResponse> SendAsync(
        Guid userId,
        SendOtpRequest request)
    {
        var user = await _dbContext.Users
            .SingleOrDefaultAsync(x => x.Id == userId);

        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        if (user.PhoneVerified)
        {
            throw new InvalidOperationException(
                "Phone number is already verified.");
        }

        // IMPORTANT:
        // OTP always goes to the phone saved during registration.
        // Do not allow this endpoint to change the account phone.
        var phone = NormalizePhone(user.Phone);

        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new InvalidOperationException(
                "No phone number is registered with this account.");
        }

        // Extra protection in case old duplicate data exists.
        var duplicatePhoneExists =
            await _dbContext.Users.AnyAsync(x =>
                x.Id != userId &&
                x.Phone == phone);

        if (duplicatePhoneExists)
        {
            throw new InvalidOperationException(
                "This phone number is already linked with another account.");
        }

        // Invalidate previous unused OTPs.
        var previousOtps =
            await _dbContext.PhoneOtps
                .Where(x =>
                    x.UserId == userId &&
                    x.UsedAt == null)
                .ToListAsync();

        foreach (var previousOtp in previousOtps)
        {
            previousOtp.UsedAt = DateTime.UtcNow;
        }

        var code = RandomNumberGenerator
            .GetInt32(100000, 1000000)
            .ToString();

        var expiresAt =
            DateTime.UtcNow.AddMinutes(
                ExpiryMinutes);

        var otp = new PhoneOtp
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CodeHash = HashCode(code),
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            FailedAttempts = 0
        };

        // Keep normalized registered number.
        user.Phone = phone;
        user.UpdatedAt = DateTime.UtcNow;

        _dbContext.PhoneOtps.Add(otp);

        await _dbContext.SaveChangesAsync();

        await _smsService.SendOtpAsync(
            phone,
            code);

        return new SendOtpResponse
        {
            Message = "OTP sent successfully.",
            ExpiresAt = expiresAt,

            // Never expose OTP outside Development.
            DevelopmentOtp =
                _environment.IsDevelopment()
                    ? code
                    : null
        };
    }

    public async Task<AuthUserResponse> VerifyAsync(
        Guid userId,
        VerifyOtpRequest request)
    {
        var user = await _dbContext.Users
            .SingleOrDefaultAsync(x => x.Id == userId);

        if (user is null)
        {
            throw new KeyNotFoundException(
                "User not found.");
        }

        if (user.PhoneVerified)
        {
            return MapUser(user);
        }

        if (string.IsNullOrWhiteSpace(
            request.Code))
        {
            throw new InvalidOperationException(
                "OTP is required.");
        }

        var code = request.Code.Trim();

        if (code.Length != 6 ||
            !code.All(char.IsDigit))
        {
            throw new InvalidOperationException(
                "Please enter a valid 6 digit OTP.");
        }

        var otp =
            await _dbContext.PhoneOtps
                .Where(x =>
                    x.UserId == userId &&
                    x.UsedAt == null)
                .OrderByDescending(
                    x => x.CreatedAt)
                .FirstOrDefaultAsync();

        if (otp is null)
        {
            throw new InvalidOperationException(
                "No active OTP found. Please request a new OTP.");
        }

        if (otp.ExpiresAt <=
            DateTime.UtcNow)
        {
            otp.UsedAt =
                DateTime.UtcNow;

            await _dbContext
                .SaveChangesAsync();

            throw new InvalidOperationException(
                "OTP has expired. Please request a new OTP.");
        }

        if (otp.FailedAttempts >=
            MaxFailedAttempts)
        {
            otp.UsedAt =
                DateTime.UtcNow;

            await _dbContext
                .SaveChangesAsync();

            throw new InvalidOperationException(
                "Too many failed attempts. Please request a new OTP.");
        }

        var suppliedHash =
            HashCode(code);

        if (!CryptographicOperations
            .FixedTimeEquals(
                Encoding.UTF8.GetBytes(
                    otp.CodeHash),
                Encoding.UTF8.GetBytes(
                    suppliedHash)))
        {
            otp.FailedAttempts++;

            if (otp.FailedAttempts >=
                MaxFailedAttempts)
            {
                otp.UsedAt =
                    DateTime.UtcNow;
            }

            await _dbContext
                .SaveChangesAsync();

            throw new InvalidOperationException(
                "Invalid OTP.");
        }

        otp.UsedAt = DateTime.UtcNow;

        user.PhoneVerified = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapUser(user);
    }

    private static string HashCode(
        string code)
    {
        var bytes =
            SHA256.HashData(
                Encoding.UTF8.GetBytes(
                    code));

        return Convert.ToHexString(
            bytes);
    }

    private static string? NormalizePhone(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(
            value))
        {
            return null;
        }

        var trimmed = value.Trim();

        var digits = new string(
            trimmed
                .Where(char.IsDigit)
                .ToArray());

        if (string.IsNullOrWhiteSpace(
            digits))
        {
            return null;
        }

        if (trimmed.StartsWith("+"))
        {
            return $"+{digits}";
        }

        if (trimmed.StartsWith("00"))
        {
            return $"+{digits[2..]}";
        }

        if (digits.StartsWith("03") &&
            digits.Length == 11)
        {
            return $"+92{digits[1..]}";
        }

        if (digits.StartsWith("92"))
        {
            return $"+{digits}";
        }

        throw new InvalidOperationException(
            "Please enter the phone number with country code, for example +923001234567.");
    }

    private static AuthUserResponse MapUser(
        User user)
    {
        return new AuthUserResponse
        {
            Id = user.Id,
            ReferenceNo = user.ReferenceNo,
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
}