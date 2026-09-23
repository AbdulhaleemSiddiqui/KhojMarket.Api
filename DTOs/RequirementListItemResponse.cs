namespace KhojMarket.Api.DTOs;

public class RequirementListItemResponse
{
    public int InquiryCount { get; set; }
    public Guid Id { get; set; }

    public string ReferenceNo { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Category { get; set; }

    public string? SubCategory { get; set; }

    public string RequirementType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string ActivityStatus { get; set; } = string.Empty;

    public decimal? BudgetMin { get; set; }

    public decimal? BudgetMax { get; set; }

    public string? Country { get; set; }

    public string? City { get; set; }

    public string? Area { get; set; }

    public string? CoverImagePath { get; set; }

    public DateTime CreatedAt { get; set; }
}