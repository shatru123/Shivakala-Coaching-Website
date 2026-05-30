namespace Shivakala.Core.ViewModels;

public sealed class AboutPageViewModel
{
    public SeoViewModel Seo { get; set; } = new();

    public IReadOnlyList<HighlightViewModel> Milestones { get; set; } = [];

    public IReadOnlyList<FacultyMemberViewModel> FacultyMembers { get; set; } = [];
}
