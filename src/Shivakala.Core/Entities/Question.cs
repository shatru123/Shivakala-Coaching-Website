namespace Shivakala.Core.Entities;

public sealed class Question : BaseEntity
{
    public required string QuestionText { get; set; }
    public string? QuestionImageUrl    { get; set; }
    public string  QuestionType        { get; set; } = "MCQ"; // MCQ
    public int     Marks               { get; set; } = 1;
    public double  NegativeMarks       { get; set; } = 0.0;
    public string? Explanation         { get; set; }
    public required string Subject      { get; set; }
    public required string Standard     { get; set; }
    public string? Topic               { get; set; }
    public string  Difficulty          { get; set; } = "Medium"; // Easy | Medium | Hard
    public bool    IsActive            { get; set; } = true;
    public DateTime CreatedDate        { get; set; } = DateTime.UtcNow;
    public string? CreatedBy           { get; set; }

    public ICollection<QuestionOption> Options { get; set; } = [];
    public ICollection<ExamQuestion> ExamQuestions { get; set; } = [];
}
