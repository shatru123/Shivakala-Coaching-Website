namespace Shivakala.Core.Entities;

public sealed class OnlineExamAttemptAnswer : BaseEntity
{
    public int AttemptId         { get; set; }
    public int QuestionId        { get; set; }
    public int? SelectedOptionId { get; set; }
    public bool IsMarkedForReview{ get; set; } = false;
    public DateTime AnsweredAt   { get; set; } = DateTime.UtcNow;

    public OnlineExamAttempt? Attempt       { get; set; }
    public Question?          Question      { get; set; }
    public QuestionOption?    SelectedOption{ get; set; }
}
