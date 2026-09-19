namespace Shivakala.Core.Entities;

/// <summary>
/// Exam registration record for an MPSC candidate. Supports both Online and Offline modes,
/// and offline venue attendance / check-in tracking.
/// </summary>
public sealed class MpscExamRegistration : BaseEntity
{
    public int ExamId              { get; set; }
    public int MpscStudentId       { get; set; }
    public DateTime RegisteredAt   { get; set; } = DateTime.UtcNow;
    public string Status           { get; set; } = "Registered"; // Registered | Eligible | Completed | Cancelled
    public string Mode             { get; set; } = "Online";     // Online | Offline

    // Offline Exam Check-In / Attendance
    public string? SeatNumber      { get; set; }
    public string AttendanceStatus { get; set; } = "Pending";    // Pending | Present | Absent
    public DateTime? AttendanceMarkedAt { get; set; }
    public string? AttendanceRemarks    { get; set; }

    public Exam? Exam              { get; set; }
    public MpscStudent? MpscStudent { get; set; }
}
