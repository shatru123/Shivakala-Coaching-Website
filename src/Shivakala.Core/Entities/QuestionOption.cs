namespace Shivakala.Core.Entities;

public sealed class QuestionOption : BaseEntity
{
    public int     QuestionId     { get; set; }
    public required string OptionText { get; set; }
    public string? OptionImageUrl { get; set; }
    public string  OptionKey      { get; set; } = "A"; // A, B, C, D
    public bool    IsCorrect      { get; set; } = false;
    public int     DisplayOrder   { get; set; } = 1;

    public Question? Question     { get; set; }
}
