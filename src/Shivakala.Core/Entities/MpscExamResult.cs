namespace Shivakala.Core.Entities;

/// <summary>
/// Result record for an MPSC candidate participating in either Online or Offline examinations.
/// </summary>
public sealed class MpscExamResult : BaseEntity
{
    public int      ExamId         { get; set; }
    public int      MpscStudentId  { get; set; }
    public double?  MarksObtained  { get; set; }
    public int?     Rank           { get; set; }
    public string?  Grade          { get; set; }
    public bool     IsAbsent       { get; set; } = false;
    public string?  Remarks        { get; set; }
    public string   Mode           { get; set; } = "Offline"; // Online | Offline
    public int?     OnlineAttemptId { get; set; }
    public DateTime CreatedDate    { get; set; } = DateTime.UtcNow;

    public Exam?    Exam           { get; set; }
    public MpscStudent? MpscStudent { get; set; }
    public OnlineExamAttempt? OnlineAttempt { get; set; }
}
