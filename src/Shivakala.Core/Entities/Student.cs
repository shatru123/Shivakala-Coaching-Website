namespace Shivakala.Core.Entities;

public sealed class Student : BaseEntity
{
    public required string FullName { get; set; }
    public string? ParentName { get; set; }
    public required string Mobile { get; set; }
    public string? Email { get; set; }
    public required string Standard { get; set; }
    public required string Subject { get; set; }
    public required string Address { get; set; }
    public string? Board { get; set; }
    public string? Medium { get; set; }
    public string Status { get; set; } = "Pending";  // Pending | Admitted | Rejected | Waitlisted
    public string? AdminNotes { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
