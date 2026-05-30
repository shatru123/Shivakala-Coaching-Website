using System.Globalization;
using Shivakala.Core.Entities;
using Shivakala.Core.ViewModels;

namespace Shivakala.Core.Common;

public static class CultureExtensions
{
    public static bool IsMarathi(this CultureInfo cultureInfo)
        => cultureInfo.TwoLetterISOLanguageName.Equals("mr", StringComparison.OrdinalIgnoreCase);

    public static CourseCardViewModel ToViewModel(this Course course, CultureInfo cultureInfo)
        => new()
        {
            Id = course.Id,
            Slug = course.Slug,
            Title = cultureInfo.IsMarathi() ? course.TitleMarathi : course.Title,
            Description = cultureInfo.IsMarathi() ? course.DescriptionMarathi : course.Description,
            Standard = course.Standard,
            DurationMonths = course.DurationMonths,
            IsFeatured = course.IsFeatured
        };
}
