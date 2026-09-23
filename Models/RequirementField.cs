namespace KhojMarket.Api.Models;

public class RequirementField
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid RequirementId { get; set; }

    public Requirement Requirement { get; set; } = null!;

    public string FieldKey { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public bool IsRequired { get; set; }

    public int SortOrder { get; set; }
}