namespace Shivakala.Core.Entities;

public sealed class Enquiry : BaseEntity
{
    public required string Name { get; set; }
    public required string Mobile { get; set; }
    public string? Email { get; set; }
    public required string Message { get; set; }
    public string? ClassInterested { get; set; }
    public bool IsRead { get; set; } = false;
    public string? AdminReply { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
