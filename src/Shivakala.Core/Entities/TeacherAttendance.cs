namespace Shivakala.Core.Entities;

public sealed class TeacherAttendance : BaseEntity
{
    public int      TeacherId   { get; set; }
    public DateOnly Date        { get; set; }
    public string   Status      { get; set; } = "Present"; // Present | Absent | HalfDay | Leave
    public string?  Remarks     { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public Teacher? Teacher { get; set; }
}
