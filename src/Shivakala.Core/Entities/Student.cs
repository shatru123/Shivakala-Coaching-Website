namespace Shivakala.Core.Entities;

public sealed class Student : BaseEntity
{
    public required string FullName         { get; set; }
    public string?   ParentName             { get; set; }
    public string?   ParentMobile           { get; set; }
    public string?   ParentEmail            { get; set; }
    public required string Mobile           { get; set; }
    public string?   Email                  { get; set; }
    public required string Standard         { get; set; }
    public required string Subject          { get; set; }
    public required string Address          { get; set; }
    public string?   Board                  { get; set; }
    public string?   Medium                 { get; set; }
    public string?   AdmissionNumber        { get; set; }
    public string?   RollNumber             { get; set; }
    public string?   PhotoUrl               { get; set; }
    public DateOnly? DateOfBirth            { get; set; }
    public string?   EmergencyContact       { get; set; }
    public string?   PreviousSchool         { get; set; }
    public string    Status                 { get; set; } = "Pending"; // Pending | Admitted | Rejected | Waitlisted
    public string?   AdminNotes             { get; set; }
    public DateTime  CreatedDate            { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<StudentBatch>        StudentBatches      { get; set; } = [];
    public ICollection<Attendance>          Attendances         { get; set; } = [];
    public ICollection<FeePayment>          FeePayments         { get; set; } = [];
    public ICollection<ExamResult>          ExamResults         { get; set; } = [];
    public ICollection<HomeworkSubmission>  HomeworkSubmissions { get; set; } = [];
}
