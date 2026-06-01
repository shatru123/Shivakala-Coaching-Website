namespace Shivakala.Core.Entities;

public sealed class Testimonial : BaseEntity
{
    public required string Name { get; set; }
    public required string Role { get; set; }         // e.g. "Parent of Std 10 Student"
    public required string Quote { get; set; }
    public string? QuoteMarathi { get; set; }
    public int Rating { get; set; } = 5;              // 1-5
    public bool IsApproved { get; set; } = false;
    public bool IsFeatured { get; set; } = false;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
