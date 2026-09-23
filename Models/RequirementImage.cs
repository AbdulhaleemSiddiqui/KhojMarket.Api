namespace KhojMarket.Api.Models;

public class RequirementImage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid RequirementId { get; set; }

    public Requirement Requirement { get; set; } = null!;

    public string FilePath { get; set; } = string.Empty;

    public bool IsCover { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}