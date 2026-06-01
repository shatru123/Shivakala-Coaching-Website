using Shivakala.Core.Entities;

namespace Shivakala.Core.Interfaces;

public interface IHomeworkRepository
{
    Task<IReadOnlyList<Homework>> GetAllAsync(string? standard = null, string? subject = null, CancellationToken ct = default);
    Task<Homework?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Homework> AddAsync(Homework homework, CancellationToken ct = default);
    Task UpdateAsync(Homework homework, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<HomeworkSubmission>> GetSubmissionsAsync(int homeworkId, CancellationToken ct = default);
    Task AddSubmissionAsync(HomeworkSubmission submission, CancellationToken ct = default);
}
