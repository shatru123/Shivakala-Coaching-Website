namespace Shivakala.Core.Entities;

public sealed class ExamQuestion : BaseEntity
{
    public int ExamId                 { get; set; }
    public int QuestionId             { get; set; }
    public int DisplayOrder           { get; set; } = 1;
    public int? MarksOverride         { get; set; }
    public double? NegativeMarksOverride { get; set; }

    public Exam? Exam                 { get; set; }
    public Question? Question         { get; set; }
}
