using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;
using Shivakala.Infrastructure.Data;

namespace Shivakala.Infrastructure.Repositories;

public sealed class AttendanceRepository(ShivakalaDbContext db) : IAttendanceRepository
{
    public Task<IReadOnlyList<Attendance>> GetByBatchAndDateAsync(int batchId, DateOnly date, CancellationToken ct)
        => db.Attendances.Include(a => a.Student)
             .Where(a => a.BatchId == batchId && a.Date == date)
             .ToListAsync(ct).ContinueWith(t => (IReadOnlyList<Attendance>)t.Result, ct);

    public Task<IReadOnlyList<Attendance>> GetByStudentAsync(int studentId, DateOnly from, DateOnly to, CancellationToken ct)
        => db.Attendances.Where(a => a.StudentId == studentId && a.Date >= from && a.Date <= to)
             .OrderByDescending(a => a.Date).ToListAsync(ct)
             .ContinueWith(t => (IReadOnlyList<Attendance>)t.Result, ct);

    public async Task BulkUpsertAsync(IEnumerable<Attendance> records, CancellationToken ct)
    {
        foreach (var r in records)
        {
            var existing = await db.Attendances.FirstOrDefaultAsync(
                a => a.StudentId == r.StudentId && a.BatchId == r.BatchId
                  && a.Date == r.Date && a.Subject == r.Subject, ct);
            if (existing is null)
                db.Attendances.Add(r);
            else
            {
                existing.Status = r.Status;
                existing.Remarks = r.Remarks;
                db.Attendances.Update(existing);
            }
        }
        await db.SaveChangesAsync(ct);
    }

    public async Task<double> GetAttendancePercentageAsync(int studentId, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var total = await db.Attendances.CountAsync(
            a => a.StudentId == studentId && a.Date >= from && a.Date <= to, ct);
        if (total == 0) return 0;
        var present = await db.Attendances.CountAsync(
            a => a.StudentId == studentId && a.Date >= from && a.Date <= to
              && (a.Status == "Present" || a.Status == "Late"), ct);
        return Math.Round((double)present / total * 100, 1);
    }

    public async Task<Dictionary<int, double>> GetBatchAttendanceSummaryAsync(int batchId, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var records = await db.Attendances
            .Where(a => a.BatchId == batchId && a.Date >= from && a.Date <= to)
            .GroupBy(a => a.StudentId)
            .Select(g => new {
                StudentId = g.Key,
                Total = g.Count(),
                Present = g.Count(a => a.Status == "Present" || a.Status == "Late")
            }).ToListAsync(ct);

        return records.ToDictionary(
            r => r.StudentId,
            r => r.Total == 0 ? 0 : Math.Round((double)r.Present / r.Total * 100, 1));
    }
}
