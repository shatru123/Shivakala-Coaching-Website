namespace Shivakala.Core.Entities;

public sealed class TimetableSlot : BaseEntity
{
    public int      BatchId     { get; set; }
    public int?     TeacherId   { get; set; }
    public required string Subject { get; set; }
    public int      DayOfWeek   { get; set; }   // 1=Mon … 7=Sun
    public TimeOnly StartTime   { get; set; }
    public TimeOnly EndTime     { get; set; }
    public string?  Room        { get; set; }
    public bool     IsActive    { get; set; } = true;

    public Batch?   Batch   { get; set; }
    public Teacher? Teacher { get; set; }
}
