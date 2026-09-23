namespace KhojMarket.Api.DTOs;

public class RequirementListQueryRequest
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }

    public string? Category { get; set; }

    public string? RequirementType { get; set; }

    public string? Country { get; set; }

    public string? City { get; set; }

    public string? Area { get; set; }

    public string? PostalCode { get; set; }

    public decimal? BudgetMin { get; set; }

    public decimal? BudgetMax { get; set; }

    // Supported values:
    // today | 7days | 30days | last7days | last30days
    public string? PostedDate { get; set; }

    // Mainly for buyer's own requirements
    public string? Status { get; set; }

    public string? ActivityStatus { get; set; }
}
