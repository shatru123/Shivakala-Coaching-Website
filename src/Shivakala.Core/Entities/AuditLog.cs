namespace Shivakala.Core.Entities;

public sealed class AuditLog : BaseEntity
{
    public required string Action       { get; set; }  // Created | Updated | Deleted | Login | Export
    public required string EntityType   { get; set; }  // Student | Teacher | FeePayment | …
    public int?     EntityId            { get; set; }
    public string?  OldValues           { get; set; }  // JSON snapshot
    public string?  NewValues           { get; set; }  // JSON snapshot
    public string?  PerformedByUsername { get; set; }
    public string?  IpAddress           { get; set; }
    public DateTime CreatedDate         { get; set; } = DateTime.UtcNow;
}
