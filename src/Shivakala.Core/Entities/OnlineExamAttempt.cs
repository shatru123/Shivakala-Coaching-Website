namespace Shivakala.Core.Entities;

public sealed class OnlineExamAttempt : BaseEntity
{
    public int      ExamId           { get; set; }
    public int?     StudentId        { get; set; }
    public int?     MpscStudentId    { get; set; }
    public int      AttemptNumber    { get; set; } = 1;
    public DateTime StartedAt        { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt     { get; set; }
    public DateTime ExpiresAt        { get; set; }
    public string   Status           { get; set; } = "InProgress"; // InProgress | Submitted | AutoSubmitted | Expired
    public double   Score            { get; set; } = 0.0;
    public int      TotalMarks       { get; set; } = 0;
    public int      CorrectCount     { get; set; } = 0;
    public int      IncorrectCount   { get; set; } = 0;
    public int      UnansweredCount  { get; set; } = 0;
    public double   Percentage       { get; set; } = 0.0;
    public int      TimeTakenSeconds { get; set; } = 0;
    public bool     IsAutoSubmitted  { get; set; } = false;
    public DateTime CreatedDate      { get; set; } = DateTime.UtcNow;

    public Exam?        Exam             { get; set; }
    public Student?     Student          { get; set; }
    public MpscStudent? MpscStudent      { get; set; }
    public ICollection<OnlineExamAttemptAnswer> Answers { get; set; } = [];
}
