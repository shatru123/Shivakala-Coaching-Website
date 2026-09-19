namespace Shivakala.Core.Entities;

public sealed class Exam : BaseEntity
{
    // ── Existing Properties (Retained for Offline Exams & Historical Results) ──
    public required string Title        { get; set; }
    public required string Standard     { get; set; }
    public required string Subject      { get; set; }
    public int       TotalMarks         { get; set; } = 100;
    public int       PassingMarks       { get; set; } = 35;
    public DateTime  ExamDate           { get; set; }
    public string?   Duration           { get; set; }   // Display duration string e.g. "2 hrs"
    public int?      BatchId            { get; set; }
    public string    ExamType           { get; set; } = "Weekly"; // Weekly | Monthly | Half-Yearly | Annual
    public bool      IsPublished        { get; set; } = false;
    public DateTime  CreatedDate        { get; set; } = DateTime.UtcNow;

    // ── Online Examination Engine Extensions ──────────────────────────────
    public string    ExamMode           { get; set; } = "Offline"; // Offline | Online
    public string?   Description        { get; set; }
    public string?   Instructions       { get; set; }
    public int       DurationMinutes    { get; set; } = 30;
    public DateTime? RegistrationOpenDate  { get; set; }
    public DateTime? RegistrationCloseDate { get; set; }
    public DateTime? StartDateTime         { get; set; }
    public DateTime? EndDateTime           { get; set; }
    public int       QuestionCount         { get; set; } = 0;
    public bool      NegativeMarkingEnabled { get; set; } = false;
    public double    NegativeMarks          { get; set; } = 0.0;
    public bool      RandomizeQuestions     { get; set; } = false;
    public bool      RandomizeOptions       { get; set; } = false;
    public bool      AllowMultipleAttempts  { get; set; } = false;
    public int       MaximumAttempts        { get; set; } = 1;
    public string    ResultReleaseMode      { get; set; } = "Immediate"; // Immediate | AfterExamEnds | Manual
    public bool      ShowCorrectAnswers     { get; set; } = true;
    public bool      IsRegistrationRequired { get; set; } = true;

    // ── Audience & Offline Examination Details ─────────────────────────────
    public string    Audience               { get; set; } = "School"; // School | Mpsc | Both
    public string?   Venue                  { get; set; }
    public int?      Capacity               { get; set; }

    // ── Navigation Properties ──────────────────────────────────────────────
    public Batch?    Batch              { get; set; }
    public ICollection<ExamResult> Results { get; set; } = [];
    public ICollection<ExamQuestion> ExamQuestions { get; set; } = [];
    public ICollection<ExamRegistration> Registrations { get; set; } = [];
    public ICollection<OnlineExamAttempt> Attempts { get; set; } = [];
    public ICollection<MpscExamRegistration> MpscRegistrations { get; set; } = [];
    public ICollection<MpscExamResult> MpscResults { get; set; } = [];
}
