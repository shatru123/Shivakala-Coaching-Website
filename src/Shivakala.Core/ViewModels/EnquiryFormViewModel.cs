using System.ComponentModel.DataAnnotations;

namespace Shivakala.Core.ViewModels;

public sealed class EnquiryFormViewModel
{
    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^[0-9]{10}$")]
    public string Mobile { get; set; } = string.Empty;

    [Required]
    [StringLength(600)]
    public string Message { get; set; } = string.Empty;

    public SeoViewModel Seo { get; set; } = new();
}
