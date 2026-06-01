namespace Shivakala.Core.Entities;

public sealed class Notice : BaseEntity
{
    public required string Title { get; set; }
    public required string TitleMarathi { get; set; }
    public required string Body { get; set; }
    public required string BodyMarathi { get; set; }
    public string Category { get; set; } = "General"; // General | Exam | Holiday | Result | Admission
    public bool IsPinned { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime PublishedDate { get; set; } = DateTime.UtcNow;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
