using KhojMarket.Api.Data;
using KhojMarket.Api.DTOs;
using KhojMarket.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KhojMarket.Api.Services;

public class SellerCreditService
{
    private readonly KhojMarketDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public SellerCreditService(
        KhojMarketDbContext dbContext,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    private int InitialCredits =>
        _configuration.GetValue<int>(
            "MarketplaceCredits:InitialSellerCredits");

    public int InquiryCost =>
        _configuration.GetValue<int>(
            "MarketplaceCredits:InquiryCost");

    public async Task<SellerWallet> GetOrCreateWalletAsync(
        Guid sellerUserId)
    {
        var wallet =
            await _dbContext.SellerWallets
                .SingleOrDefaultAsync(x =>
                    x.SellerUserId == sellerUserId);

        if (wallet is not null)
        {
            return wallet;
        }

        var sellerExists =
            await _dbContext.Users
                .AnyAsync(x =>
                    x.Id == sellerUserId &&
                    x.Role == "seller");

        if (!sellerExists)
        {
            throw new KeyNotFoundException(
                "Seller not found.");
        }

        var now = DateTime.UtcNow;

        wallet = new SellerWallet
        {
            Id = Guid.NewGuid(),
            SellerUserId = sellerUserId,
            Balance = InitialCredits,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.SellerWallets.Add(wallet);

        _dbContext.CreditTransactions.Add(
            new CreditTransaction
            {
                Id = Guid.NewGuid(),
                SellerUserId = sellerUserId,
                Type = "initial_balance",
                Amount = InitialCredits,
                BalanceAfter = InitialCredits,
                Description =
                    "Initial seller credit balance.",
                CreatedAt = now
            });

        await _dbContext.SaveChangesAsync();

        return wallet;
    }

    public async Task<SellerWalletResponse> GetWalletAsync(
        Guid sellerUserId)
    {
        var wallet =
            await GetOrCreateWalletAsync(
                sellerUserId);

        return new SellerWalletResponse
        {
            Balance = wallet.Balance,

            InquiryCost = InquiryCost,

            CanSendInquiry =
                wallet.Balance >= InquiryCost
        };
    }

    public async Task DeductInquiryCreditsAsync(
        Guid sellerUserId,
        Guid inquiryId)
    {
        var wallet =
            await GetOrCreateWalletAsync(
                sellerUserId);

        if (InquiryCost <= 0)
        {
            throw new InvalidOperationException(
                "Inquiry credit cost is not configured correctly.");
        }

        if (wallet.Balance < InquiryCost)
        {
            throw new InvalidOperationException(
                $"Insufficient credits. Inquiry requires {InquiryCost} credits.");
        }

        wallet.Balance -= InquiryCost;
        wallet.UpdatedAt = DateTime.UtcNow;

        _dbContext.CreditTransactions.Add(
            new CreditTransaction
            {
                Id = Guid.NewGuid(),

                SellerUserId =
                    sellerUserId,

                InquiryId =
                    inquiryId,

                Type =
                    "inquiry_deduction",

                Amount =
                    -InquiryCost,

                BalanceAfter =
                    wallet.Balance,

                Description =
                    $"Credits deducted for inquiry {inquiryId}.",

                CreatedAt =
                    DateTime.UtcNow
            });
    }

    public async Task<List<CreditTransactionResponse>>
        GetTransactionsAsync(
            Guid sellerUserId)
    {
        await GetOrCreateWalletAsync(
            sellerUserId);

        return await _dbContext.CreditTransactions
            .AsNoTracking()
            .Where(x =>
                x.SellerUserId == sellerUserId)
            .OrderByDescending(x =>
                x.CreatedAt)
            .Select(x =>
                new CreditTransactionResponse
                {
                    Id = x.Id,

                    InquiryId =
                        x.InquiryId,

                    Type =
                        x.Type,

                    Amount =
                        x.Amount,

                    BalanceAfter =
                        x.BalanceAfter,

                    Description =
                        x.Description,

                    CreatedAt =
                        x.CreatedAt
                })
            .ToListAsync();
    }
    public async Task<SellerWalletResponse> AdjustCreditsAsync(
    Guid adminUserId,
    Guid sellerUserId,
    AdminAdjustCreditsRequest request)
    {
        if (request.Amount == 0)
        {
            throw new InvalidOperationException(
                "Amount cannot be zero.");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new InvalidOperationException(
                "Reason is required.");
        }

        var seller = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(x =>
                x.Id == sellerUserId &&
                x.Role == "seller");

        if (seller is null)
        {
            throw new KeyNotFoundException(
                "Seller not found.");
        }

        var wallet =
            await GetOrCreateWalletAsync(
                sellerUserId);

        var newBalance =
            wallet.Balance + request.Amount;

        if (newBalance < 0)
        {
            throw new InvalidOperationException(
                "Seller balance cannot go below zero.");
        }

        wallet.Balance =
            newBalance;

        wallet.UpdatedAt =
            DateTime.UtcNow;

        var transactionType =
            request.Amount > 0
                ? "admin_credit"
                : "admin_deduction";

        _dbContext.CreditTransactions.Add(
            new CreditTransaction
            {
                Id = Guid.NewGuid(),

                SellerUserId =
                    sellerUserId,

                PerformedByUserId =
                    adminUserId,

                Type =
                    transactionType,

                Amount =
                    request.Amount,

                BalanceAfter =
                    newBalance,

                Description =
                    request.Reason.Trim(),

                CreatedAt =
                    DateTime.UtcNow
            });

        await _dbContext.SaveChangesAsync();

        return new SellerWalletResponse
        {
            Balance =
                wallet.Balance,

            InquiryCost =
                InquiryCost,

            CanSendInquiry =
                wallet.Balance >= InquiryCost
        };
    }
    public async Task<List<CreditTransactionResponse>>
    GetSellerTransactionsForAdminAsync(
        Guid sellerUserId)
    {
        var sellerExists =
            await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == sellerUserId &&
                    x.Role == "seller");

        if (!sellerExists)
        {
            throw new KeyNotFoundException(
                "Seller not found.");
        }

        return await GetTransactionsAsync(
            sellerUserId);
    }
    public async Task<SellerWalletResponse> GetSellerWalletForAdminAsync(
    Guid sellerUserId)
    {
        var seller = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(x =>
                x.Id == sellerUserId &&
                x.Role == "seller");

        if (seller is null)
        {
            throw new KeyNotFoundException(
                "Seller not found.");
        }

        return await GetWalletAsync(
            sellerUserId);
    }
}