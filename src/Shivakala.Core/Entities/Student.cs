namespace Shivakala.Core.Entities;

public sealed class Student : BaseEntity
{
    public required string FullName { get; set; }

    public required string Mobile { get; set; }

    public string? Email { get; set; }

    public required string Standard { get; set; }

    public required string Subject { get; set; }

    public required string Address { get; set; }

    public DateTime CreatedDate { get; set; }
}
