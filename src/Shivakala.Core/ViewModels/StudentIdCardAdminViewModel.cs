using System.ComponentModel.DataAnnotations;

namespace Shivakala.Core.ViewModels;

public sealed class StudentIdCardAdminViewModel
{
    public int Id { get; set; }

    [Required]
    public string FullName { get; set; } = string.Empty;

    public string? ParentName { get; set; }

    [Required]
    public string Mobile { get; set; } = string.Empty;

    public string? ParentMobile { get; set; }

    public string? Email { get; set; }

    [Required]
    public string Standard { get; set; } = string.Empty;

    [Required]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;

    public string? Board { get; set; }

    public string? Medium { get; set; }

    public string Status { get; set; } = "Pending";

    [Display(Name = "Admission Number")]
    public string? AdmissionNumber { get; set; }

    [Display(Name = "Roll Number")]
    public string? RollNumber { get; set; }

    [Display(Name = "Date of Birth")]
    public string? DateOfBirth { get; set; }

    public string? PhotoUrl { get; set; }

    [Display(Name = "Emergency Contact")]
    public string? EmergencyContact { get; set; }

    [Display(Name = "Previous School")]
    public string? PreviousSchool { get; set; }

    public DateTime CreatedDate { get; set; }
}
