namespace KhojMarket.Api.DTOs;

public sealed class GenerateAiRequirementRequest
{
    public string Text { get; set; } = string.Empty;
}

public sealed class AiRequirementResponse
{
    public string Category { get; set; } = string.Empty;
    public string? SubCategory { get; set; }
    public string Title { get; set; } = string.Empty;
    public string RequirementType { get; set; } = "product";
    public List<AiRequirementFieldResponse> Fields { get; set; } = new();
}

public sealed class AiRequirementFieldResponse
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool Required { get; set; }
}
