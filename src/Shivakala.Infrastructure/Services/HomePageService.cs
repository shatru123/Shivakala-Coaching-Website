using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shivakala.Core.Common;
using Shivakala.Core.Entities;
using Shivakala.Core.Services;
using Shivakala.Core.ViewModels;
using Shivakala.Infrastructure.Data;
using Shivakala.Infrastructure.Repositories;

namespace Shivakala.Infrastructure.Services;

public sealed class HomePageService(
    ICourseService courseService,
    ShivakalaDbContext db,
    ITestimonialRepository testimonialRepo,
    ILogger<HomePageService> logger) : IHomePageService
{
    public async Task<HomePageViewModel> GetHomePageAsync(CancellationToken cancellationToken = default)
    {
        var isMarathi = CultureInfo.CurrentUICulture.IsMarathi();
        var featuredCourses = await courseService.GetFeaturedCoursesAsync(cancellationToken);
        var settings = await GetSettingsAsync(cancellationToken);
        var testimonials = await testimonialRepo.GetApprovedAsync(featuredOnly: true, ct: cancellationToken);
        if (testimonials.Count == 0)
            testimonials = await testimonialRepo.GetApprovedAsync(ct: cancellationToken);

        return new HomePageViewModel
        {
            Seo = new SeoViewModel
            {
                Title = "Shivakala Coaching Classes | SSC, Foundation & Scholarship Coaching",
                Description = "Production-ready coaching website for Shivakala Coaching Classes with multilingual admissions and enquiry experience.",
                Keywords = "Shivakala Coaching Classes, SSC coaching, foundation batch, scholarship preparation, Marathi coaching website"
            },
            FeaturedCourses = featuredCourses,
            HeroBannerImageUrl = string.IsNullOrWhiteSpace(settings.HeroBannerImageUrl) ? "/img/Banner.jpeg" : settings.HeroBannerImageUrl,
            HeroBannerAltText = string.IsNullOrWhiteSpace(settings.HeroBannerAltText) ? "Shivakala Classes admissions banner" : settings.HeroBannerAltText,
            ShowTrendingBanner = settings.ShowTrendingBanner,
            TrendingEyebrow = isMarathi
                ? (string.IsNullOrWhiteSpace(settings.TrendingEyebrowMarathi) ? "नवीन अपडेट" : settings.TrendingEyebrowMarathi)
                : (string.IsNullOrWhiteSpace(settings.TrendingEyebrow) ? "Trending Now" : settings.TrendingEyebrow),
            TrendingTitle = isMarathi
                ? (string.IsNullOrWhiteSpace(settings.TrendingTitleMarathi) ? "नवीन शैक्षणिक वर्षासाठी प्रवेश सुरू" : settings.TrendingTitleMarathi)
                : (string.IsNullOrWhiteSpace(settings.TrendingTitle) ? "Admissions open for the new academic year" : settings.TrendingTitle),
            TrendingDescription = isMarathi
                ? (string.IsNullOrWhiteSpace(settings.TrendingDescriptionMarathi) ? "महत्त्वाच्या घोषणा, बॅचेस, ऑफर्स किंवा इव्हेंट्स अॅडमिन पॅनलमधून लगेच दाखवा." : settings.TrendingDescriptionMarathi)
                : (string.IsNullOrWhiteSpace(settings.TrendingDescription) ? "Highlight important announcements, batches, offers, or events right from the admin panel." : settings.TrendingDescription),
            TrendingImageUrl = string.IsNullOrWhiteSpace(settings.TrendingImageUrl) ? "/img/Banner.jpeg" : settings.TrendingImageUrl,
            TrendingAltText = string.IsNullOrWhiteSpace(settings.TrendingAltText) ? "Trending banner for Shivakala Coaching Classes" : settings.TrendingAltText,
            TrendingLinkText = isMarathi
                ? (string.IsNullOrWhiteSpace(settings.TrendingLinkTextMarathi) ? "अधिक जाणून घ्या" : settings.TrendingLinkTextMarathi)
                : (string.IsNullOrWhiteSpace(settings.TrendingLinkText) ? "Explore Now" : settings.TrendingLinkText),
            TrendingLinkUrl = string.IsNullOrWhiteSpace(settings.TrendingLinkUrl) ? "/registration" : settings.TrendingLinkUrl,
            ShowStatisticsSection = settings.ShowStatisticsSection,
            Statistics =
            [
                new() { Value = settings.Stat1Value, Label = isMarathi ? settings.Stat1LabelMarathi : settings.Stat1Label },
                new() { Value = settings.Stat2Value, Label = isMarathi ? settings.Stat2LabelMarathi : settings.Stat2Label },
                new() { Value = settings.Stat3Value, Label = isMarathi ? settings.Stat3LabelMarathi : settings.Stat3Label },
                new() { Value = settings.Stat4Value, Label = isMarathi ? settings.Stat4LabelMarathi : settings.Stat4Label }
            ],
            Highlights =
            [
                new() { Icon = "fa-solid fa-book-open-reader", Title = isMarathi ? "संकल्पनांवर भर" : "Concept-first teaching", Description = isMarathi ? "मूलभूत संकल्पना स्पष्ट करून दीर्घकालीन समज वाढवतो." : "We build deep understanding through concept clarity and structured revision." },
                new() { Icon = "fa-solid fa-chart-line", Title = isMarathi ? "नियमित प्रगती विश्लेषण" : "Regular performance tracking", Description = isMarathi ? "चाचण्या, विश्लेषण आणि पालक संवादामुळे सातत्य टिकते." : "Frequent tests, analytics, and parent updates keep performance improving." },
                new() { Icon = "fa-solid fa-user-group", Title = isMarathi ? "लहान बॅचेस" : "Focused small batches", Description = isMarathi ? "प्रत्येक विद्यार्थ्याकडे वैयक्तिक लक्ष देण्यासाठी नियोजित बॅच रचना." : "Smaller batch sizes ensure each student gets personalised guidance." }
            ],
            Results =
            [
                new() { Icon = "fa-solid fa-trophy", Title = isMarathi ? "SSC टॉपर परंपरा" : "Consistent SSC toppers", Description = isMarathi ? "गेल्या काही वर्षांत अनेक विद्यार्थ्यांनी 90% पेक्षा जास्त गुण मिळवले." : "Our students consistently secure 90%+ scores across SSC boards." },
                new() { Icon = "fa-solid fa-bullseye", Title = isMarathi ? "शिष्यवृत्ती यश" : "Scholarship success", Description = isMarathi ? "स्पर्धात्मक परीक्षांसाठी विशेष सराव आणि रणनीती." : "Targeted preparation helps scholarship aspirants compete confidently." },
                new() { Icon = "fa-solid fa-lightbulb", Title = isMarathi ? "दैनंदिन शंका समाधान" : "Daily doubt solving", Description = isMarathi ? "शंका राहू नयेत म्हणून वेगवेगळ्या सपोर्ट सत्रांची रचना." : "Dedicated doubt-solving sessions keep learning momentum strong." }
            ],
            ShowTestimonialsSection = settings.ShowTestimonialsSection,
            TestimonialsEyebrow = isMarathi ? settings.TestimonialsEyebrowMarathi : settings.TestimonialsEyebrow,
            TestimonialsTitle = isMarathi ? settings.TestimonialsTitleMarathi : settings.TestimonialsTitle,
            Testimonials = testimonials
                .Take(3)
                .Select(t => new TestimonialViewModel
                {
                    StudentName = t.Name,
                    Achievement = t.Role,
                    Quote = isMarathi && !string.IsNullOrWhiteSpace(t.QuoteMarathi) ? t.QuoteMarathi : t.Quote,
                    Rating = t.Rating
                })
                .ToList(),
            FacultyMembers =
            [
                new() { Name = "Prof. Shrikant Sir", Designation = isMarathi ? "संस्थापक आणि गणित मार्गदर्शक" : "Founder & Mathematics Mentor", Experience = isMarathi ? "15+ वर्षे" : "15+ years", Speciality = isMarathi ? "बोर्ड, स्कॉलरशिप, ऑलिंपियाड" : "Boards, scholarships, olympiads" },
                new() { Name = "Mrs. Kavita Ma'am", Designation = isMarathi ? "सायन्स तज्ज्ञ" : "Science Specialist", Experience = isMarathi ? "12+ वर्षे" : "12+ years", Speciality = isMarathi ? "प्रायोगिक संकल्पना आणि रिव्हिजन" : "Practical concepts and revision strategy" },
                new() { Name = "Mr. Nilesh Sir", Designation = isMarathi ? "इंग्रजी आणि टेस्ट स्ट्रॅटेजी" : "English & Test Strategy", Experience = isMarathi ? "10+ वर्षे" : "10+ years", Speciality = isMarathi ? "भाषिक कौशल्य आणि लेखन" : "Language skills and writing improvement" }
            ]
        };
    }

    private async Task<HomePageSectionSettings> GetSettingsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var settings = await db.HomePageSectionSettings.FirstOrDefaultAsync(cancellationToken);
            if (settings is not null) return settings;

            settings = new HomePageSectionSettings();
            db.HomePageSectionSettings.Add(settings);
            await db.SaveChangesAsync(cancellationToken);
            return settings;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Falling back to in-code homepage defaults because homepage content settings are unavailable.");
            return new HomePageSectionSettings();
        }
    }
}
