namespace Shivakala.Core.Entities;

public sealed class Enquiry : BaseEntity
{
    public required string Name { get; set; }

    public required string Mobile { get; set; }

    public required string Message { get; set; }

    public DateTime CreatedDate { get; set; }
}
