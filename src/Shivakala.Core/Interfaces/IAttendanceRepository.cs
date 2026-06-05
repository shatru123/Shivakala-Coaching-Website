using Shivakala.Core.Entities;

namespace Shivakala.Core.Interfaces;

public interface IAttendanceRepository
{
    Task<IReadOnlyList<Attendance>> GetByBatchAndDateAsync(int batchId, DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyList<Attendance>> GetByStudentAsync(int studentId, DateOnly from, DateOnly to, CancellationToken ct = default);
    Task BulkUpsertAsync(IEnumerable<Attendance> records, CancellationToken ct = default);
    Task<double> GetAttendancePercentageAsync(int studentId, DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<Dictionary<int, double>> GetBatchAttendanceSummaryAsync(int batchId, DateOnly from, DateOnly to, CancellationToken ct = default);
}
