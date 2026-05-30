using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;
using Shivakala.Infrastructure.Data;

namespace Shivakala.Infrastructure.Repositories;

public sealed class StudentRepository(ShivakalaDbContext dbContext) : Repository<Student>(dbContext), IStudentRepository
{
    public async Task<IReadOnlyList<Student>> ListRecentAsync(CancellationToken cancellationToken = default)
        => await DbContext.Students
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);
}
