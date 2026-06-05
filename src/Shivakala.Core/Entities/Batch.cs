namespace Shivakala.Core.Entities;

public sealed class Batch : BaseEntity
{
    public required string Name          { get; set; }   // e.g. "8th Morning Batch A"
    public required string Standard      { get; set; }
    public string?   Medium              { get; set; }   // English | Marathi | Semi-English
    public int       MaxStrength         { get; set; } = 30;
    public string?   Room                { get; set; }
    public string?   TimingSlot          { get; set; }   // "07:00-09:00"
    public bool      IsActive            { get; set; } = true;
    public string    AcademicYear        { get; set; } = $"{DateTime.UtcNow.Year}-{DateTime.UtcNow.Year + 1}";
    public DateTime  CreatedDate         { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<BatchSubject>    BatchSubjects    { get; set; } = [];
    public ICollection<StudentBatch>    StudentBatches   { get; set; } = [];
    public ICollection<TimetableSlot>   TimetableSlots   { get; set; } = [];
}
