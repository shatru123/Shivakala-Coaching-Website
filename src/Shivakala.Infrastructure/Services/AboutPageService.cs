using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Common;
using Shivakala.Core.Entities;
using Shivakala.Core.Services;
using Shivakala.Core.ViewModels;
using Shivakala.Infrastructure.Data;

namespace Shivakala.Infrastructure.Services;

public sealed class AboutPageService(ShivakalaDbContext db) : IAboutPageService
{
    public async Task<AboutPageViewModel> GetAboutPageAsync(CancellationToken cancellationToken = default)
    {
        var isMarathi = CultureInfo.CurrentUICulture.IsMarathi();
        var settings = await GetSettingsAsync(cancellationToken);
        var facultyMembers = await db.Teachers
            .Where(t => t.IsActive && t.ShowOnAboutPage)
            .OrderBy(t => t.JoiningDate)
            .ThenBy(t => t.FullName)
            .Select(t => new FacultyMemberViewModel
            {
                Name = t.FullName,
                PhotoUrl = t.PhotoUrl ?? string.Empty,
                Designation = isMarathi
                    ? (t.PublicDesignationMarathi ?? t.PublicDesignation ?? t.Qualification ?? "शिक्षक")
                    : (t.PublicDesignation ?? t.Qualification ?? "Faculty Mentor"),
                Experience = ResolveExperience(t, isMarathi),
                Speciality = t.Specialisation ?? string.Empty
            })
            .ToListAsync(cancellationToken);

        return new AboutPageViewModel
        {
            Seo = new SeoViewModel
            {
                Title = "About Shivakala Coaching Classes",
                Description = "Learn about Shivakala Coaching Classes, our academic vision, and our experienced faculty.",
                Keywords = "about Shivakala Coaching Classes, faculty, coaching institute Maharashtra"
            },
            ShowStatisticsSection = settings.ShowStatisticsSection,
            Statistics =
            [
                new() { Value = settings.Stat1Value, Label = isMarathi ? settings.Stat1LabelMarathi : settings.Stat1Label },
                new() { Value = settings.Stat2Value, Label = isMarathi ? settings.Stat2LabelMarathi : settings.Stat2Label },
                new() { Value = settings.Stat3Value, Label = isMarathi ? settings.Stat3LabelMarathi : settings.Stat3Label },
                new() { Value = settings.Stat4Value, Label = isMarathi ? settings.Stat4LabelMarathi : settings.Stat4Label }
            ],
            Milestones =
            [
                new() { Icon = "fa-solid fa-school", Title = isMarathi ? "मजबूत शैक्षणिक पाया" : "Strong academic foundation", Description = isMarathi ? "स्थानिक विद्यार्थ्यांसाठी संकल्पनांवर आधारित प्रशिक्षण." : "Concept-driven coaching designed for local students with ambitious goals." },
                new() { Icon = "fa-solid fa-people-group", Title = isMarathi ? "पालक-सहभाग मॉडेल" : "Parent partnership model", Description = isMarathi ? "नियमित संवादामुळे विद्यार्थी, शिक्षक आणि पालक एकाच दिशेने काम करतात." : "Frequent communication aligns students, mentors, and parents on progress." },
                new() { Icon = "fa-solid fa-medal", Title = isMarathi ? "निकालकेंद्री संस्कृती" : "Results-led culture", Description = isMarathi ? "टेस्ट सीरिज, मार्गदर्शन आणि सातत्यपूर्ण फीडबॅक ही आमची वैशिष्ट्ये." : "Test series, mentoring, and fast feedback create measurable outcomes." }
            ],
            FacultyMembers = facultyMembers,
            Address = isMarathi ? settings.AddressMarathi : settings.Address,
            MapEmbedUrl = settings.MapEmbedUrl
        };
    }

    private async Task<AboutPageSectionSettings> GetSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await db.Set<AboutPageSectionSettings>().FirstOrDefaultAsync(cancellationToken);
        if (settings is not null) return settings;

        settings = new AboutPageSectionSettings();
        db.Add(settings);
        await db.SaveChangesAsync(cancellationToken);
        return settings;
    }

    private static string ResolveExperience(Teacher teacher, bool isMarathi)
    {
        var configuredValue = isMarathi ? teacher.PublicExperienceMarathi : teacher.PublicExperience;
        if (!string.IsNullOrWhiteSpace(configuredValue))
            return configuredValue;

        var years = Math.Max(1, DateTime.UtcNow.Year - teacher.JoiningDate.Year);
        return isMarathi ? $"{years}+ वर्षे अनुभव" : $"{years}+ years of experience";
    }
}
