namespace Shivakala.Core.ViewModels;

public sealed class HomePageViewModel
{
    public SeoViewModel Seo { get; set; } = new();

    public IReadOnlyList<CourseCardViewModel> FeaturedCourses { get; set; } = [];

    public IReadOnlyList<StatisticViewModel> Statistics { get; set; } = [];

    public bool ShowStatisticsSection { get; set; } = true;

    public IReadOnlyList<HighlightViewModel> Highlights { get; set; } = [];

    public IReadOnlyList<HighlightViewModel> Results { get; set; } = [];

    public IReadOnlyList<TestimonialViewModel> Testimonials { get; set; } = [];

    public bool ShowTestimonialsSection { get; set; } = true;

    public string TestimonialsEyebrow { get; set; } = "Testimonials";

    public string TestimonialsTitle { get; set; } = "What Parents & Students Say";

    public IReadOnlyList<FacultyMemberViewModel> FacultyMembers { get; set; } = [];
}
