namespace Shivakala.Core.ViewModels;

public sealed class AboutPageViewModel
{
    public SeoViewModel Seo { get; set; } = new();

    public bool ShowStatisticsSection { get; set; } = true;

    public IReadOnlyList<StatisticViewModel> Statistics { get; set; } = [];

    public IReadOnlyList<HighlightViewModel> Milestones { get; set; } = [];

    public IReadOnlyList<FacultyMemberViewModel> FacultyMembers { get; set; } = [];

    public string Address { get; set; } = string.Empty;

    public string MapEmbedUrl { get; set; } = string.Empty;
}
