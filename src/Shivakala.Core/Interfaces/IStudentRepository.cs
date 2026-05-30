using Shivakala.Core.Entities;

namespace Shivakala.Core.Interfaces;

public interface IStudentRepository : IRepository<Student>
{
    Task<IReadOnlyList<Student>> ListRecentAsync(CancellationToken cancellationToken = default);
}
