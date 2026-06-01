namespace Shivakala.Core.Entities;

public sealed class ExamResult : BaseEntity
{
    public int      ExamId      { get; set; }
    public int      StudentId   { get; set; }
    public int?     MarksObtained { get; set; }
    public string?  Grade       { get; set; }
    public int?     Rank        { get; set; }
    public bool     IsAbsent    { get; set; } = false;
    public string?  Remarks     { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public Exam?    Exam    { get; set; }
    public Student? Student { get; set; }
}
