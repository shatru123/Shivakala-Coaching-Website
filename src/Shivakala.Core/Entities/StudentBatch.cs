namespace Shivakala.Core.Entities;

public sealed class StudentBatch : BaseEntity
{
    public int      StudentId   { get; set; }
    public int      BatchId     { get; set; }
    public DateTime JoinDate    { get; set; } = DateTime.UtcNow;
    public bool     IsActive    { get; set; } = true;

    public Student? Student { get; set; }
    public Batch?   Batch   { get; set; }
}
