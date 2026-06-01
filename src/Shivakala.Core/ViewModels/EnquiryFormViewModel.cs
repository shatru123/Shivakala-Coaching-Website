using System.ComponentModel.DataAnnotations;

namespace Shivakala.Core.ViewModels;

public sealed class EnquiryFormViewModel
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mobile number is required.")]
    [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Enter a valid 10-digit mobile number.")]
    public string Mobile { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(150)]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Message is required.")]
    [StringLength(600)]
    public string Message { get; set; } = string.Empty;

    [StringLength(40)]
    public string? ClassInterested { get; set; }

    public SeoViewModel Seo { get; set; } = new();
}
