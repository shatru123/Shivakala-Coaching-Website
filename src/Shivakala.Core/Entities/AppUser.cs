namespace Shivakala.Core.Entities;

/// <summary>Represents a portal user (Admin, Teacher, Student, Parent).</summary>
public sealed class AppUser : BaseEntity
{
    public required string Username   { get; set; }
    public required string Email      { get; set; }
    public required string PasswordHash { get; set; }
    public required string Role       { get; set; }     // SuperAdmin | Admin | Teacher | Student | Parent
    public string?   FullName         { get; set; }
    public string?   Mobile           { get; set; }
    public string?   PhotoUrl         { get; set; }
    public bool      IsActive         { get; set; } = true;
    public string?   PasswordResetToken     { get; set; }
    public DateTime? PasswordResetExpiry    { get; set; }
    public DateTime  CreatedDate      { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginDate    { get; set; }

    // FK mappings (nullable — a user may not yet be linked)
    public int? TeacherId  { get; set; }
    public int? StudentId  { get; set; }
}
