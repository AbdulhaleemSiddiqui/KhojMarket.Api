using KhojMarket.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KhojMarket.Api.Data;

public class KhojMarketDbContext : DbContext
{
    public KhojMarketDbContext(
        DbContextOptions<KhojMarketDbContext> options)
        : base(options)
    {
    }
    public DbSet<MarketplaceDeal> MarketplaceDeals =>
    Set<MarketplaceDeal>();
    public DbSet<User> Users => Set<User>();
    public DbSet<PhoneOtp> PhoneOtps => Set<PhoneOtp>();
    public DbSet<SellerInquiry> SellerInquiries =>
    Set<SellerInquiry>();
    public DbSet<SellerWallet> SellerWallets =>
    Set<SellerWallet>();
    public DbSet<MarketplaceReview> MarketplaceReviews =>
    Set<MarketplaceReview>();
    public DbSet<MarketplaceComplaint> MarketplaceComplaints => Set<MarketplaceComplaint>();
    public DbSet<UserWarning> UserWarnings => Set<UserWarning>();

    public DbSet<CreditTransaction> CreditTransactions =>
        Set<CreditTransaction>();
    public DbSet<Requirement> Requirements => Set<Requirement>();
    public DbSet<RequirementFavorite> RequirementFavorites =>
    Set<RequirementFavorite>();
    public DbSet<RequirementField> RequirementFields => Set<RequirementField>();

    public DbSet<RequirementImage> RequirementImages => Set<RequirementImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.ReferenceNo)
                .IsUnique();

            entity.HasIndex(x => x.Email)
               .IsUnique();

            entity.HasIndex(x => x.Phone)
                .IsUnique()
                .HasFilter("[Phone] IS NOT NULL");

            entity.Property(x => x.ReferenceNo)
                .HasMaxLength(50);

            entity.Property(x => x.Role)
                .HasMaxLength(20);

            entity.Property(x => x.SellerVerificationStatus)
                .HasMaxLength(30);
        });
        modelBuilder.Entity<RequirementFavorite>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new
            {
                x.UserId,
                x.RequirementId
            })
            .IsUnique();

            entity.HasIndex(x => x.RequirementId);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Requirement)
                .WithMany()
                .HasForeignKey(x => x.RequirementId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Requirement>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.ReferenceNo)
                .IsUnique();

            entity.HasIndex(x => x.UserId);

            entity.HasIndex(x => x.Status);

            entity.HasIndex(x => x.ActivityStatus);

            entity.HasIndex(x => x.Category);

            entity.HasIndex(x => x.City);

            entity.Property(x => x.ReferenceNo)
                .HasMaxLength(50);

            entity.Property(x => x.RequirementType)
                .HasMaxLength(20);

            entity.Property(x => x.Status)
                .HasMaxLength(40);

            entity.Property(x => x.ActivityStatus)
                .HasMaxLength(20);

            entity.Property(x => x.BudgetMin)
                .HasPrecision(18, 2);

            entity.Property(x => x.BudgetMax)
                .HasPrecision(18, 2);

            entity.HasOne(x => x.User)
                .WithMany(x => x.Requirements)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<SellerInquiry>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Message)
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.OfferedPrice)
                .HasPrecision(18, 2);

            entity.HasIndex(x => new
            {
                x.RequirementId,
                x.SellerUserId
            })
            .IsUnique();

            entity.HasIndex(x => x.BuyerUserId);

            entity.HasIndex(x => x.SellerUserId);

            entity.HasIndex(x => x.Status);

            entity.HasOne(x => x.Requirement)
                .WithMany(x => x.Inquiries)
                .HasForeignKey(x => x.RequirementId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.BuyerUser)
                .WithMany()
                .HasForeignKey(x => x.BuyerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SellerUser)
                .WithMany()
                .HasForeignKey(x => x.SellerUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<RequirementField>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.RequirementId);

            entity.Property(x => x.FieldKey)
                .HasMaxLength(100);

            entity.Property(x => x.Label)
                .HasMaxLength(150);

            entity.HasOne(x => x.Requirement)
                .WithMany(x => x.Fields)
                .HasForeignKey(x => x.RequirementId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<PhoneOtp>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.UserId);

            entity.HasIndex(x => x.ExpiresAt);

            entity.Property(x => x.CodeHash)
                .HasMaxLength(200);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<RequirementImage>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.RequirementId);

            entity.HasOne(x => x.Requirement)
                .WithMany(x => x.Images)
                .HasForeignKey(x => x.RequirementId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<SellerWallet>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.SellerUserId)
                .IsUnique();

            entity.HasOne(x => x.SellerUser)
                .WithMany()
                .HasForeignKey(x => x.SellerUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<MarketplaceDeal>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Status)
                .HasMaxLength(30)
                .IsRequired();

            entity.HasIndex(x => x.RequirementId)
                .IsUnique();

            entity.HasIndex(x => x.InquiryId)
                .IsUnique();

            entity.HasIndex(x => x.BuyerUserId);

            entity.HasIndex(x => x.SellerUserId);

            entity.HasOne(x => x.Requirement)
                .WithMany()
                .HasForeignKey(x => x.RequirementId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Inquiry)
                .WithMany()
                .HasForeignKey(x => x.InquiryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.BuyerUser)
                .WithMany()
                .HasForeignKey(x => x.BuyerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SellerUser)
                .WithMany()
                .HasForeignKey(x => x.SellerUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<CreditTransaction>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Type)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500)
                .IsRequired();

            entity.HasIndex(x => x.SellerUserId);

            entity.HasIndex(x => x.InquiryId);

            entity.HasIndex(x => x.CreatedAt);
        });
        modelBuilder.Entity<MarketplaceComplaint>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Category).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Details).HasMaxLength(1500).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.Property(x => x.AdminNote).HasMaxLength(1000);
            entity.HasIndex(x => x.ReportedUserId);
            entity.HasIndex(x => x.Status);
            entity.HasOne(x => x.ReporterUser).WithMany().HasForeignKey(x => x.ReporterUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ReportedUser).WithMany().HasForeignKey(x => x.ReportedUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Deal).WithMany().HasForeignKey(x => x.DealId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<UserWarning>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.ComplaintId).IsUnique();
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Complaint).WithMany().HasForeignKey(x => x.ComplaintId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<MarketplaceReview>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Comment)
                .HasMaxLength(1000);

            entity.HasIndex(x => new
            {
                x.DealId,
                x.ReviewerUserId
            })
            .IsUnique();

            entity.HasIndex(x => x.RevieweeUserId);

            entity.HasOne(x => x.Deal)
                .WithMany()
                .HasForeignKey(x => x.DealId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.ReviewerUser)
                .WithMany()
                .HasForeignKey(x => x.ReviewerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RevieweeUser)
                .WithMany()
                .HasForeignKey(x => x.RevieweeUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}