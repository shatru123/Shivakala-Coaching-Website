using Shivakala.Core.Entities;

namespace Shivakala.Core.Interfaces;

public interface IExamRepository
{
    Task<IReadOnlyList<Exam>> GetAllAsync(CancellationToken ct = default);
    Task<Exam?> GetByIdWithResultsAsync(int id, CancellationToken ct = default);
    Task<Exam> AddAsync(Exam exam, CancellationToken ct = default);
    Task UpdateAsync(Exam exam, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task BulkUpsertResultsAsync(IEnumerable<ExamResult> results, CancellationToken ct = default);
    Task RecalculateRanksAsync(int examId, CancellationToken ct = default);
}
