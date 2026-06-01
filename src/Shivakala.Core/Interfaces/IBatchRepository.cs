using Shivakala.Core.Entities;

namespace Shivakala.Core.Interfaces;

public interface IBatchRepository
{
    Task<IReadOnlyList<Batch>> GetAllAsync(CancellationToken ct = default);
    Task<Batch?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<Batch> AddAsync(Batch batch, CancellationToken ct = default);
    Task UpdateAsync(Batch batch, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<int> GetStudentCountAsync(int batchId, CancellationToken ct = default);
}
