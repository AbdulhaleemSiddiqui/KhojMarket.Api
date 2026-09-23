namespace KhojMarket.Api.DTOs;

public class CreateRequirementFieldRequest
{
    public string FieldKey { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public bool IsRequired { get; set; }

    public int SortOrder { get; set; }
}