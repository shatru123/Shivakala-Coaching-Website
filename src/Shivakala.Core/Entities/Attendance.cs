namespace Shivakala.Core.Entities;

public sealed class Attendance : BaseEntity
{
    public int      StudentId   { get; set; }
    public int      BatchId     { get; set; }
    public string?  Subject     { get; set; }           // null = whole-day attendance
    public DateOnly Date        { get; set; }
    public string   Status      { get; set; } = "Present"; // Present | Absent | Late | Holiday
    public string?  Remarks     { get; set; }
    public int?     MarkedByTeacherId { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public Student? Student { get; set; }
    public Batch?   Batch   { get; set; }
    public Teacher? MarkedByTeacher { get; set; }
}
