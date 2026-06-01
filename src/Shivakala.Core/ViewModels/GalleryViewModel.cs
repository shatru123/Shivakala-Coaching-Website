namespace Shivakala.Core.ViewModels;

public sealed class GalleryItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public string Category { get; set; } = "General";
    public int DisplayOrder { get; set; }
}

public sealed class GalleryPageViewModel
{
    public IReadOnlyList<GalleryItemViewModel> Items { get; set; } = [];
    public IReadOnlyList<string> Categories { get; set; } = [];
    public string? SelectedCategory { get; set; }
    public SeoViewModel Seo { get; set; } = new();
}
