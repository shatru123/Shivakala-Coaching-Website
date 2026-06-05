namespace Shivakala.Core.Entities;

public sealed class SyllabusItem : BaseEntity
{
    public required string Standard     { get; set; }
    public required string Subject      { get; set; }
    public required string ChapterName  { get; set; }
    public int?     BatchId             { get; set; }
    public int?     TeacherId           { get; set; }
    public int      DisplayOrder        { get; set; }
    public bool     IsCompleted         { get; set; } = false;
    public DateTime? CompletedDate      { get; set; }
    public string?  AttachmentUrl       { get; set; }
    public string?  Notes               { get; set; }
    public DateTime CreatedDate         { get; set; } = DateTime.UtcNow;

    public Batch?   Batch   { get; set; }
    public Teacher? Teacher { get; set; }
}
