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
    private readonly ISmsService _smsService;
    private const int MaxFailedAttempts = 5;

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

        var phone = NormalizePhone(request.Phone);

        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new InvalidOperationException(
                "Valid phone number is required.");
        }

        if (user.PhoneVerified)
        {
            throw new InvalidOperationException(
                "Phone number is already verified.");
        }

        // Invalidate previous unused OTPs.
        var previousOtps = await _dbContext.PhoneOtps
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
            DateTime.UtcNow.AddMinutes(ExpiryMinutes);

        var otp = new PhoneOtp
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CodeHash = HashCode(code),
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            FailedAttempts = 0
        };

        // Phone becomes the account phone being verified.
        user.Phone = phone;
        user.UpdatedAt = DateTime.UtcNow;

        _dbContext.PhoneOtps.Add(otp);

        await _dbContext.SaveChangesAsync();

        // Later:
         await _smsService.SendOtpAsync(phone, code);

        return new SendOtpResponse
        {
            Message = "OTP sent successfully.",
            ExpiresAt = expiresAt,

            // NEVER return OTP in production.
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
            throw new KeyNotFoundException("User not found.");
        }

        if (user.PhoneVerified)
        {
            return MapUser(user);
        }

        var otp = await _dbContext.PhoneOtps
            .Where(x =>
                x.UserId == userId &&
                x.UsedAt == null)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (otp is null)
        {
            throw new InvalidOperationException(
                "No active OTP found. Please request a new OTP.");
        }

        if (otp.ExpiresAt <= DateTime.UtcNow)
        {
            otp.UsedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            throw new InvalidOperationException(
                "OTP has expired. Please request a new OTP.");
        }

        if (otp.FailedAttempts >= MaxFailedAttempts)
        {
            otp.UsedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            throw new InvalidOperationException(
                "Too many failed attempts. Please request a new OTP.");
        }

        var suppliedHash =
            HashCode(request.Code.Trim());

        if (!CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(otp.CodeHash),
            Encoding.UTF8.GetBytes(suppliedHash)))
        {
            otp.FailedAttempts++;

            if (otp.FailedAttempts >= MaxFailedAttempts)
            {
                otp.UsedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            throw new InvalidOperationException(
                "Invalid OTP.");
        }

        otp.UsedAt = DateTime.UtcNow;

        user.PhoneVerified = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapUser(user);
    }

    private static string HashCode(string code)
    {
        var bytes =
            SHA256.HashData(
                Encoding.UTF8.GetBytes(code));

        return Convert.ToHexString(bytes);
    }

    private static string NormalizePhone(string phone)
    {
        var value = phone
            .Trim()
            .Replace(" ", "")
            .Replace("-", "");

        // Pakistan normalization
        if (value.StartsWith("03"))
        {
            value = "+92" + value[1..];
        }
        else if (value.StartsWith("92"))
        {
            value = "+" + value;
        }

        return value;
    }

    private static AuthUserResponse MapUser(User user)
    {
        return new AuthUserResponse
        {
            Id = user.Id,
            ReferenceNo = user.ReferenceNo,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role,
            PhoneVerified = user.PhoneVerified,
            SellerVerificationStatus =
                user.SellerVerificationStatus
        };
    }
}