namespace Shivakala.Core.Entities;

public sealed class TestResult : BaseEntity
{
    public required string StudentName { get; set; }
    public required string Standard { get; set; }
    public required string Subject { get; set; }
    public int Score { get; set; }
    public int TotalMarks { get; set; }
    public int Rank { get; set; }
    public string? Grade { get; set; }
    public string? Remarks { get; set; }
    public DateTime TestDate { get; set; }
    public required string TestTitle { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
