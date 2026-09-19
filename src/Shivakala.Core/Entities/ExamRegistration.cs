namespace Shivakala.Core.Entities;

public sealed class ExamRegistration : BaseEntity
{
    public int ExamId           { get; set; }
    public int StudentId        { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public string Status        { get; set; } = "Registered"; // Registered | Eligible | Completed | Cancelled
    public int AttemptCount     { get; set; } = 0;

    public Exam? Exam           { get; set; }
    public Student? Student     { get; set; }
}
