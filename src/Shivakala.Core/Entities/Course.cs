namespace Shivakala.Core.Entities;

public sealed class Course : BaseEntity
{
    public required string Slug { get; set; }

    public required string Title { get; set; }

    public required string TitleMarathi { get; set; }

    public required string Description { get; set; }

    public required string DescriptionMarathi { get; set; }

    public required string Standard { get; set; }

    public int DurationMonths { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsFeatured { get; set; }
}
