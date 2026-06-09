namespace Shivakala.Core.ViewModels;

public sealed class AboutPageContentAdminViewModel
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
    public string Address { get; set; } = string.Empty;
    public string AddressMarathi { get; set; } = string.Empty;
    public string MapEmbedUrl { get; set; } = string.Empty;
}
