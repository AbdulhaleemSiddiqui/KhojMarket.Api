namespace KhojMarket.Api.DTOs;

public class RequirementResponse
{
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

    public string? PostalCode { get; set; }

    public bool PhoneVerified { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<RequirementFieldResponse> Fields { get; set; } = new();

    public List<RequirementImageResponse> Images { get; set; } = new();
}

public class RequirementFieldResponse
{
    public Guid Id { get; set; }

    public string FieldKey { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public bool IsRequired { get; set; }

    public int SortOrder { get; set; }
}

public class RequirementImageResponse
{
    public Guid Id { get; set; }

    public string FilePath { get; set; } = string.Empty;

    public bool IsCover { get; set; }

    public int SortOrder { get; set; }
}