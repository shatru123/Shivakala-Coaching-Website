namespace Shivakala.Core.ViewModels;

public sealed class StudentAdminViewModel
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Mobile { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string Standard { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }
}
