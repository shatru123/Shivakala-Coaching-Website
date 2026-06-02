namespace Shivakala.Core.Entities;

/// <summary>
/// Student profile entity.
/// Original columns (from InitialCreate + AddNewFeatures migrations) are non-nullable.
/// New columns (from AddManagementSystem migration) are all nullable.
/// </summary>
public sealed class Student : BaseEntity
{
    // ── Original columns (already exist in DB) ────────────────────────────
    public required string FullName     { get; set; }
    public string?   ParentName         { get; set; }
    public required string Mobile       { get; set; }
    public string?   Email              { get; set; }
    public required string Standard     { get; set; }
    public required string Subject      { get; set; }
    public required string Address      { get; set; }
    public string?   Board              { get; set; }
    public string?   Medium             { get; set; }
    public string    Status             { get; set; } = "Pending";
    public string?   AdminNotes         { get; set; }
    public DateTime  CreatedDate        { get; set; } = DateTime.UtcNow;

    // ── New columns (added by AddManagementSystem migration) ──────────────
    public string?   AdmissionNumber    { get; set; }
    public string?   RollNumber         { get; set; }
    public string?   PhotoUrl           { get; set; }
    public string?   ParentMobile       { get; set; }
    public string?   ParentEmail        { get; set; }
    public string?   EmergencyContact   { get; set; }
    public string?   PreviousSchool     { get; set; }
    public string?   DateOfBirth        { get; set; }   // stored as TEXT "yyyy-MM-dd"

    // ── Navigation properties (lazy — no cascade issues) ──────────────────
    public ICollection<StudentBatch>        StudentBatches      { get; set; } = [];
    public ICollection<Attendance>          Attendances         { get; set; } = [];
    public ICollection<FeePayment>          FeePayments         { get; set; } = [];
    public ICollection<ExamResult>          ExamResults         { get; set; } = [];
    public ICollection<HomeworkSubmission>  HomeworkSubmissions { get; set; } = [];
}
