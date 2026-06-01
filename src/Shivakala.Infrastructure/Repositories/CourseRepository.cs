using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;
using Shivakala.Infrastructure.Data;

namespace Shivakala.Infrastructure.Repositories;

public sealed class CourseRepository(ShivakalaDbContext dbContext) : Repository<Course>(dbContext), ICourseRepository
{
    public async Task<IReadOnlyList<Course>> ListFeaturedAsync(CancellationToken cancellationToken = default)
        => await DbContext.Courses
            .AsNoTracking()
            .Where(x => x.IsFeatured)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(cancellationToken);

    public override async Task<IReadOnlyList<Course>> ListAsync(CancellationToken cancellationToken = default)
        => await DbContext.Courses
            .AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(cancellationToken);

    public async Task<Course?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        => await DbContext.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

    public async Task UpdateAsync(Course course, CancellationToken cancellationToken = default)
    {
        DbContext.Courses.Update(course);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var course = await DbContext.Courses.FindAsync([id], cancellationToken);
        if (course == null)
        {
            return;
        }

        DbContext.Courses.Remove(course);
        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
