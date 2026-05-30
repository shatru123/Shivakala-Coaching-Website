namespace Shivakala.Core.ViewModels;

public sealed class CourseCardViewModel
{
    public int Id { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Standard { get; set; } = string.Empty;

    public int DurationMonths { get; set; }

    public bool IsFeatured { get; set; }
}
