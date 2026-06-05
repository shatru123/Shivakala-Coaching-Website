namespace Shivakala.Core.Entities;

public sealed class BatchSubject : BaseEntity
{
    public int       BatchId     { get; set; }
    public required string Subject { get; set; }
    public int?      TeacherId   { get; set; }

    // Navigation
    public Batch?   Batch   { get; set; }
    public Teacher? Teacher { get; set; }
}
