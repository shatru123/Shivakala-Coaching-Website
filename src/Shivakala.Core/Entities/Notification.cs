namespace Shivakala.Core.Entities;

public sealed class Notification : BaseEntity
{
    public required string Title    { get; set; }
    public required string Message  { get; set; }
    public required string Channel  { get; set; }   // InApp | WhatsApp | Email | SMS
    public required string Audience { get; set; }   // All | Batch:{id} | Student:{id} | Parent:{id}
    public string   Status          { get; set; } = "Pending"; // Pending | Sent | Failed
    public string?  TemplateKey     { get; set; }
    public string?  AttachmentUrl   { get; set; }
    public int?     SentByUserId    { get; set; }
    public int      DeliveredCount  { get; set; } = 0;
    public int      FailedCount     { get; set; } = 0;
    public DateTime? SentAt         { get; set; }
    public DateTime CreatedDate     { get; set; } = DateTime.UtcNow;
}
