using Shivakala.Core.Common;

namespace Shivakala.Core.ViewModels;

public sealed class NoticeViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string TitleMarathi { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string BodyMarathi { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public bool IsPinned { get; set; }
    public bool IsActive { get; set; }
    public DateTime PublishedDate { get; set; }
}

public sealed class NoticeBoardViewModel
{
    public IReadOnlyList<NoticeViewModel> Pinned { get; set; } = [];
    public IReadOnlyList<NoticeViewModel> All { get; set; } = [];
    public string? SelectedCategory { get; set; }
    public SeoViewModel Seo { get; set; } = new();
}

public sealed class NoticeFormViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string TitleMarathi { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string BodyMarathi { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public bool IsPinned { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime PublishedDate { get; set; } = UtcDateTime.StartOfToday();
}
