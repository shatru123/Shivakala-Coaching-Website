namespace Shivakala.Core.Entities;

public sealed class HomePageSectionSettings : BaseEntity
{
    public string HeroBannerImageUrl { get; set; } = "/img/Banner.jpeg";
    public string HeroBannerAltText { get; set; } = "Shivakala Classes admissions banner";
    public bool ShowTrendingBanner { get; set; } = false;
    public string TrendingEyebrow { get; set; } = "Trending Now";
    public string TrendingEyebrowMarathi { get; set; } = "नवीन अपडेट";
    public string TrendingTitle { get; set; } = "Admissions open for the new academic year";
    public string TrendingTitleMarathi { get; set; } = "नवीन शैक्षणिक वर्षासाठी प्रवेश सुरू";
    public string TrendingDescription { get; set; } = "Highlight important announcements, batches, offers, or events right from the admin panel.";
    public string TrendingDescriptionMarathi { get; set; } = "महत्त्वाच्या घोषणा, बॅचेस, ऑफर्स किंवा इव्हेंट्स अॅडमिन पॅनलमधून लगेच दाखवा.";
    public string TrendingImageUrl { get; set; } = "/img/Banner.jpeg";
    public string TrendingAltText { get; set; } = "Trending banner for Shivakala Coaching Classes";
    public string TrendingLinkText { get; set; } = "Explore Now";
    public string TrendingLinkTextMarathi { get; set; } = "अधिक जाणून घ्या";
    public string TrendingLinkUrl { get; set; } = "/registration";
    public bool ShowStatisticsSection { get; set; } = true;
    public string Stat1Value { get; set; } = "500+";
    public string Stat1Label { get; set; } = "Students";
    public string Stat1LabelMarathi { get; set; } = "विद्यार्थी";
    public string Stat2Value { get; set; } = "10+";
    public string Stat2Label { get; set; } = "Years Experience";
    public string Stat2LabelMarathi { get; set; } = "वर्षांचा अनुभव";
    public string Stat3Value { get; set; } = "95%";
    public string Stat3Label { get; set; } = "Pass Rate";
    public string Stat3LabelMarathi { get; set; } = "उत्तीर्ण दर";
    public string Stat4Value { get; set; } = "KG-10";
    public string Stat4Label { get; set; } = "All Standards";
    public string Stat4LabelMarathi { get; set; } = "सर्व वर्ग";
    public bool ShowTestimonialsSection { get; set; } = true;
    public string TestimonialsEyebrow { get; set; } = "Testimonials";
    public string TestimonialsEyebrowMarathi { get; set; } = "विद्यार्थी व पालकांचे मत";
    public string TestimonialsTitle { get; set; } = "What Parents & Students Say";
    public string TestimonialsTitleMarathi { get; set; } = "आमच्या विद्यार्थ्यांचे अनुभव";
}
