namespace Shivakala.Core.ViewModels;

public sealed class HomePageViewModel
{
    public SeoViewModel Seo { get; set; } = new();
    public string HeroBannerImageUrl { get; set; } = "/img/Banner.jpeg";
    public string HeroBannerAltText { get; set; } = "Shivakala Classes admissions banner";
    public bool ShowTrendingBanner { get; set; }
    public string TrendingEyebrow { get; set; } = "Trending Now";
    public string TrendingTitle { get; set; } = "Admissions open for the new academic year";
    public string TrendingDescription { get; set; } = "Highlight important announcements, batches, offers, or events right from the admin panel.";
    public string TrendingImageUrl { get; set; } = "/img/Banner.jpeg";
    public string TrendingAltText { get; set; } = "Trending banner for Shivakala Coaching Classes";
    public string TrendingLinkText { get; set; } = "Explore Now";
    public string TrendingLinkUrl { get; set; } = "/registration";

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
