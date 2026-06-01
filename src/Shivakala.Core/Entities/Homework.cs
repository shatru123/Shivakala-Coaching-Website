namespace Shivakala.Core.Entities;

public sealed class Homework : BaseEntity
{
    public required string Title        { get; set; }
    public string?   Description        { get; set; }
    public required string Subject      { get; set; }
    public required string Standard     { get; set; }
    public int?      BatchId            { get; set; }
    public int       AssignedByTeacherId { get; set; }
    public DateTime  DueDate            { get; set; }
    public string?   AttachmentUrl      { get; set; }
    public bool      IsActive           { get; set; } = true;
    public DateTime  CreatedDate        { get; set; } = DateTime.UtcNow;

    public Batch?    Batch              { get; set; }
    public Teacher?  AssignedByTeacher  { get; set; }
    public ICollection<HomeworkSubmission> Submissions { get; set; } = [];
}
