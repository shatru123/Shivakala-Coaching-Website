using Shivakala.Core.ViewModels;

namespace Shivakala.Core.Services;

public interface ICourseService
{
    Task<IReadOnlyList<CourseCardViewModel>> GetCoursesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CourseCardViewModel>> GetFeaturedCoursesAsync(CancellationToken cancellationToken = default);
}
