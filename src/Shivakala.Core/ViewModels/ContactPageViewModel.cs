namespace Shivakala.Core.ViewModels;

public sealed class ContactPageViewModel
{
    public SeoViewModel Seo { get; set; } = new();

    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string WhatsAppNumber { get; set; } = string.Empty;

    public string MapQuery { get; set; } = string.Empty;
}
