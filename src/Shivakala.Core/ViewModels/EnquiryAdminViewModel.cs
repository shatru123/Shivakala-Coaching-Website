namespace Shivakala.Core.ViewModels;

public sealed class EnquiryAdminViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Mobile { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }
}
