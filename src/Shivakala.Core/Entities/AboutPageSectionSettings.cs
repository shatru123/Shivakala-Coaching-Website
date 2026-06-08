namespace Shivakala.Core.Entities;

public sealed class AboutPageSectionSettings : BaseEntity
{
    public bool ShowStatisticsSection { get; set; } = true;
    public string Stat1Value { get; set; } = "500+";
    public string Stat1Label { get; set; } = "Students Enrolled";
    public string Stat1LabelMarathi { get; set; } = "नोंदणीकृत विद्यार्थी";
    public string Stat2Value { get; set; } = "10+";
    public string Stat2Label { get; set; } = "Years of Excellence";
    public string Stat2LabelMarathi { get; set; } = "उत्कृष्टतेची वर्षे";
    public string Stat3Value { get; set; } = "95%";
    public string Stat3Label { get; set; } = "Pass Rate";
    public string Stat3LabelMarathi { get; set; } = "उत्तीर्ण दर";
    public string Stat4Value { get; set; } = "3";
    public string Stat4Label { get; set; } = "Core Subjects";
    public string Stat4LabelMarathi { get; set; } = "मुख्य विषय";
    public string Address { get; set; } = "Shivakala Coaching Classes, Chikhali, Maharashtra";
    public string AddressMarathi { get; set; } = "शिवकला कोचिंग क्लासेस, चिखली, महाराष्ट्र";
    public string MapEmbedUrl { get; set; } = "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3782.1!2d73.79!3d18.65!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x0%3A0x0!2sChikhali%2C%20Maharashtra!5e0!3m2!1sen!2sin!4v1234567890";
}
