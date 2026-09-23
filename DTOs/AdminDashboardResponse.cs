namespace KhojMarket.Api.DTOs;

public class AdminDashboardResponse
{
    public int TotalBuyers { get; set; }

    public int TotalSellers { get; set; }

    public int PendingSellers { get; set; }

    public int VerifiedSellers { get; set; }

    public int TotalRequirements { get; set; }

    public int PendingRequirements { get; set; }

    public int ActiveVerifiedRequirements { get; set; }

    public int CompletedRequirements { get; set; }

    public int TotalInquiries { get; set; }

    public int CompletedDeals { get; set; }

    public int TotalReviews { get; set; }

    public int TotalSellerCreditBalance { get; set; }
}