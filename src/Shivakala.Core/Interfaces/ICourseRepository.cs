using Shivakala.Core.Entities;

namespace Shivakala.Core.Interfaces;

public interface ICourseRepository : IRepository<Course>
{
    Task<IReadOnlyList<Course>> ListFeaturedAsync(CancellationToken cancellationToken = default);

    Task<Course?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task UpdateAsync(Course course, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
