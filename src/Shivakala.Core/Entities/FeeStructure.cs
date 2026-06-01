namespace Shivakala.Core.Entities;

public sealed class FeeStructure : BaseEntity
{
    public required string Standard      { get; set; }
    public required string FeeType       { get; set; }  // Admission | Monthly | Exam | Annual
    public decimal   Amount              { get; set; }
    public string    AcademicYear        { get; set; } = $"{DateTime.UtcNow.Year}-{DateTime.UtcNow.Year + 1}";
    public bool      IsActive            { get; set; } = true;
    public DateTime  CreatedDate         { get; set; } = DateTime.UtcNow;
}
