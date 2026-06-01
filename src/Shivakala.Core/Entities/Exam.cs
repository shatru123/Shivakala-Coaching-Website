namespace Shivakala.Core.Entities;

public sealed class Exam : BaseEntity
{
    public required string Title        { get; set; }
    public required string Standard     { get; set; }
    public required string Subject      { get; set; }
    public int       TotalMarks         { get; set; } = 100;
    public int       PassingMarks       { get; set; } = 35;
    public DateTime  ExamDate           { get; set; }
    public string?   Duration           { get; set; }   // "2 hrs"
    public int?      BatchId            { get; set; }
    public string    ExamType           { get; set; } = "Weekly"; // Weekly | Monthly | Half-Yearly | Annual
    public bool      IsPublished        { get; set; } = false;
    public DateTime  CreatedDate        { get; set; } = DateTime.UtcNow;

    public Batch?    Batch              { get; set; }
    public ICollection<ExamResult> Results { get; set; } = [];
}
