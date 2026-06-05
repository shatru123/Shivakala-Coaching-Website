using System.ComponentModel.DataAnnotations;

namespace Shivakala.Core.ViewModels;

public sealed class AdminStudentFormViewModel
{
    [Required(ErrorMessage = "Student name is required.")]
    [StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(120)]
    public string? ParentName { get; set; }

    [Required(ErrorMessage = "Mobile number is required.")]
    [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Enter a valid 10-digit mobile number.")]
    public string Mobile { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(150)]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Standard is required.")]
    [StringLength(80)]
    public string Standard { get; set; } = string.Empty;

    [Required(ErrorMessage = "Subject is required.")]
    [StringLength(120)]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(250)]
    public string Address { get; set; } = string.Empty;

    [StringLength(80)]
    public string? Board { get; set; }

    [StringLength(60)]
    public string? Medium { get; set; }

    [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Enter a valid 10-digit parent mobile.")]
    public string? ParentMobile { get; set; }

    [Required]
    public string Status { get; set; } = "Admitted";

    [StringLength(500)]
    public string? AdminNotes { get; set; }
}
