namespace Shivakala.Core.Entities;

public sealed class HomeworkSubmission : BaseEntity
{
    public int      HomeworkId  { get; set; }
    public int      StudentId   { get; set; }
    public string?  FileUrl     { get; set; }
    public string?  Notes       { get; set; }
    public string   Status      { get; set; } = "Submitted"; // Submitted | Reviewed | Late
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public Homework? Homework { get; set; }
    public Student?  Student  { get; set; }
}
