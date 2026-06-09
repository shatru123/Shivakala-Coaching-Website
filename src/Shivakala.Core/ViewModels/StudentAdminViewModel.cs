namespace Shivakala.Core.ViewModels;

public sealed class StudentAdminViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? ParentName { get; set; }
    public string Mobile { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Standard { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Board { get; set; }
    public string? Medium { get; set; }
    public string? AdmissionNumber { get; set; }
    public string? PhotoUrl { get; set; }
    public string Status { get; set; } = "Pending";
    public string? AdminNotes { get; set; }
    public DateTime CreatedDate { get; set; }
}
