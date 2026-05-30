using System.Globalization;
using Shivakala.Core.Common;
using Shivakala.Core.Interfaces;
using Shivakala.Core.Services;
using Shivakala.Core.ViewModels;

namespace Shivakala.Infrastructure.Services;

public sealed class CourseService(ICourseRepository courseRepository) : ICourseService
{
    public async Task<IReadOnlyList<CourseCardViewModel>> GetCoursesAsync(CancellationToken cancellationToken = default)
    {
        var culture = CultureInfo.CurrentUICulture;
        var courses = await courseRepository.ListAsync(cancellationToken);
        return courses.Select(x => x.ToViewModel(culture)).ToList();
    }

    public async Task<IReadOnlyList<CourseCardViewModel>> GetFeaturedCoursesAsync(CancellationToken cancellationToken = default)
    {
        var culture = CultureInfo.CurrentUICulture;
        var courses = await courseRepository.ListFeaturedAsync(cancellationToken);
        return courses.Select(x => x.ToViewModel(culture)).ToList();
    }
}
