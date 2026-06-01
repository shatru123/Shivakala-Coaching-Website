namespace Shivakala.Core.Entities;

public sealed class StudyMaterial : BaseEntity
{
    public required string Title { get; set; }
    public required string TitleMarathi { get; set; }
    public required string FileUrl { get; set; }       // relative path under wwwroot/uploads/materials/
    public required string Standard { get; set; }
    public required string Subject { get; set; }
    public string MaterialType { get; set; } = "QuestionPaper"; // QuestionPaper | AnswerSheet | Notes | Worksheet
    public long FileSizeBytes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;
}
