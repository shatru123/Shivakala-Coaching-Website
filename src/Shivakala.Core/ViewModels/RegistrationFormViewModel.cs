using System.ComponentModel.DataAnnotations;

namespace Shivakala.Core.ViewModels;

public sealed class RegistrationFormViewModel
{
    [Required]
    [StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^[0-9]{10}$")]
    public string Mobile { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(150)]
    public string? Email { get; set; }

    [Required]
    [StringLength(80)]
    public string Standard { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [StringLength(250)]
    public string Address { get; set; } = string.Empty;

    public SeoViewModel Seo { get; set; } = new();
}
