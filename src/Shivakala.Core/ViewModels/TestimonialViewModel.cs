namespace Shivakala.Core.ViewModels;

public sealed class TestimonialViewModel
{
    public string StudentName { get; set; } = string.Empty;

    public string Achievement { get; set; } = string.Empty;

    public string Quote { get; set; } = string.Empty;

    public int Rating { get; set; } = 5;
}

public sealed class TestimonialFormViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string Quote { get; set; } = string.Empty;

    public string? QuoteMarathi { get; set; }

    public int Rating { get; set; } = 5;

    public bool IsApproved { get; set; } = true;

    public bool IsFeatured { get; set; }
}
