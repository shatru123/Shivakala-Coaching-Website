namespace Shivakala.Core.Entities;

public sealed class GalleryItem : BaseEntity
{
    public required string Title { get; set; }
    public required string ImageUrl { get; set; }      // relative path under wwwroot/uploads/gallery/
    public string? Caption { get; set; }
    public string Category { get; set; } = "General"; // General | Event | Results | Classroom
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
