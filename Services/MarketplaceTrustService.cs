using KhojMarket.Api.Data;
using KhojMarket.Api.DTOs;
using KhojMarket.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KhojMarket.Api.Services;

public class MarketplaceTrustService
{
    private readonly KhojMarketDbContext _db;
    public MarketplaceTrustService(KhojMarketDbContext db) => _db = db;

    public async Task<MarketplaceComplaintResponse> ReportAsync(Guid reporterId, CreateMarketplaceComplaintRequest request)
    {
        if (reporterId == request.ReportedUserId) throw new InvalidOperationException("You cannot report yourself.");
        if (string.IsNullOrWhiteSpace(request.Category) || string.IsNullOrWhiteSpace(request.Details))
            throw new InvalidOperationException("Complaint category and details are required.");

        var reported = await _db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.ReportedUserId)
            ?? throw new KeyNotFoundException("Reported user not found.");

        if (request.DealId.HasValue)
        {
            var deal = await _db.MarketplaceDeals.AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.DealId.Value)
                ?? throw new KeyNotFoundException("Deal not found.");
            var participants = (deal.BuyerUserId == reporterId && deal.SellerUserId == reported.Id) ||
                               (deal.SellerUserId == reporterId && deal.BuyerUserId == reported.Id);
            if (!participants) throw new UnauthorizedAccessException("This deal does not belong to these users.");
        }

        var complaint = new MarketplaceComplaint
        {
            Id = Guid.NewGuid(), ReporterUserId = reporterId, ReportedUserId = reported.Id,
            DealId = request.DealId, Category = request.Category.Trim().ToLowerInvariant(),
            Details = request.Details.Trim(), Status = "pending", CreatedAt = DateTime.UtcNow
        };
        _db.MarketplaceComplaints.Add(complaint);
        await _db.SaveChangesAsync();
        return await GetAsync(complaint.Id);
    }

    public Task<List<MarketplaceComplaintResponse>> GetAdminComplaintsAsync(string? status) =>
        _db.MarketplaceComplaints.AsNoTracking()
            .Where(x => string.IsNullOrWhiteSpace(status) || status == "all" || x.Status == status)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => Map(x)).ToListAsync();

    public async Task<MarketplaceComplaintResponse> ConfirmWarningAsync(Guid adminId, Guid complaintId, ReviewMarketplaceComplaintRequest request)
    {
        var complaint = await _db.MarketplaceComplaints.SingleOrDefaultAsync(x => x.Id == complaintId)
            ?? throw new KeyNotFoundException("Complaint not found.");
        if (complaint.Status != "pending") throw new InvalidOperationException("Complaint has already been reviewed.");

        await using var tx = await _db.Database.BeginTransactionAsync();
        complaint.Status = "warning_issued";
        complaint.AdminNote = request.AdminNote.Trim();
        complaint.ReviewedByUserId = adminId;
        complaint.ReviewedAt = DateTime.UtcNow;

        _db.UserWarnings.Add(new UserWarning
        {
            Id = Guid.NewGuid(), UserId = complaint.ReportedUserId, ComplaintId = complaint.Id,
            Reason = complaint.AdminNote, IssuedByUserId = adminId, CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var warningCount = await _db.UserWarnings.CountAsync(x => x.UserId == complaint.ReportedUserId);
        if (warningCount >= 3)
        {
            var user = await _db.Users.SingleAsync(x => x.Id == complaint.ReportedUserId);
            user.IsBlocked = true;
            user.BlockedAt ??= DateTime.UtcNow;
            user.BlockReason = "Account blocked after 3 confirmed marketplace warnings.";
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        await tx.CommitAsync();
        return await GetAsync(complaint.Id);
    }

    public async Task<MarketplaceComplaintResponse> DismissAsync(Guid adminId, Guid complaintId, ReviewMarketplaceComplaintRequest request)
    {
        var complaint = await _db.MarketplaceComplaints.SingleOrDefaultAsync(x => x.Id == complaintId)
            ?? throw new KeyNotFoundException("Complaint not found.");
        if (complaint.Status != "pending") throw new InvalidOperationException("Complaint has already been reviewed.");
        complaint.Status = "dismissed"; complaint.AdminNote = request.AdminNote.Trim();
        complaint.ReviewedByUserId = adminId; complaint.ReviewedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(); return await GetAsync(complaint.Id);
    }

    public Task<List<BlockedUserResponse>> GetBlockedUsersAsync() =>
        _db.Users.AsNoTracking().Where(x => x.IsBlocked).OrderByDescending(x => x.BlockedAt)
            .Select(x => new BlockedUserResponse {
                UserId=x.Id, Name=x.Name, Role=x.Role, IsBlocked=x.IsBlocked, BlockedAt=x.BlockedAt,
                BlockReason=x.BlockReason, WarningCount=_db.UserWarnings.Count(w => w.UserId==x.Id)
            }).ToListAsync();

    public async Task UnblockAsync(Guid userId)
    {
        var user = await _db.Users.SingleOrDefaultAsync(x => x.Id == userId) ?? throw new KeyNotFoundException("User not found.");
        user.IsBlocked=false; user.BlockedAt=null; user.BlockReason=null; user.UpdatedAt=DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    private async Task<MarketplaceComplaintResponse> GetAsync(Guid id) =>
        await _db.MarketplaceComplaints.AsNoTracking().Where(x=>x.Id==id).Select(x=>Map(x)).SingleAsync();

    private static MarketplaceComplaintResponse Map(MarketplaceComplaint x) => new()
    {
        Id=x.Id, ReporterUserId=x.ReporterUserId, ReporterName=x.ReporterUser.Name,
        ReportedUserId=x.ReportedUserId, ReportedUserName=x.ReportedUser.Name, ReportedUserRole=x.ReportedUser.Role,
        DealId=x.DealId, Category=x.Category, Details=x.Details, Status=x.Status,
        AdminNote=x.AdminNote, CreatedAt=x.CreatedAt, ReviewedAt=x.ReviewedAt
    };
}
