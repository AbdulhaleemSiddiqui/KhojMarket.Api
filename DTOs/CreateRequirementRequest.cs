namespace KhojMarket.Api.DTOs;

public class CreateRequirementRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Category { get; set; }

    public string? SubCategory { get; set; }

    public string RequirementType { get; set; } = "product";

    public decimal? BudgetMin { get; set; }

    public decimal? BudgetMax { get; set; }

    public string? Country { get; set; }

    public string? City { get; set; }

    public string? Area { get; set; }

    public string? PostalCode { get; set; }

    public List<CreateRequirementFieldRequest> Fields { get; set; } = new();
}