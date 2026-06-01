namespace Shivakala.Core.ViewModels;

public sealed class EnquiryAdminViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ClassInterested { get; set; }
    public bool IsRead { get; set; }
    public string? AdminReply { get; set; }
    public DateTime CreatedDate { get; set; }
}
