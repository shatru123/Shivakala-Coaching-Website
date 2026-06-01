using Shivakala.Core.Entities;

namespace Shivakala.Core.Interfaces;

public interface ITeacherRepository
{
    Task<IReadOnlyList<Teacher>> GetAllAsync(CancellationToken ct = default);
    Task<Teacher?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Teacher> AddAsync(Teacher teacher, CancellationToken ct = default);
    Task UpdateAsync(Teacher teacher, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
