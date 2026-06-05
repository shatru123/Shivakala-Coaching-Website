namespace Shivakala.Core.Entities;

public sealed class HomePageSectionSettings : BaseEntity
{
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
