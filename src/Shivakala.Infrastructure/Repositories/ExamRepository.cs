using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;
using Shivakala.Infrastructure.Data;

namespace Shivakala.Infrastructure.Repositories;

public sealed class ExamRepository(ShivakalaDbContext db) : IExamRepository
{
    public Task<IReadOnlyList<Exam>> GetAllAsync(CancellationToken ct)
        => db.Exams.Include(e => e.Batch)
             .OrderByDescending(e => e.ExamDate).ToListAsync(ct)
             .ContinueWith(t => (IReadOnlyList<Exam>)t.Result, ct);

    public Task<Exam?> GetByIdWithResultsAsync(int id, CancellationToken ct)
        => db.Exams.Include(e => e.Results).ThenInclude(r => r.Student)
             .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<Exam> AddAsync(Exam exam, CancellationToken ct)
    { db.Exams.Add(exam); await db.SaveChangesAsync(ct); return exam; }

    public async Task UpdateAsync(Exam exam, CancellationToken ct)
    { db.Exams.Update(exam); await db.SaveChangesAsync(ct); }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var e = await db.Exams.FindAsync([id], ct);
        if (e is not null) { db.Exams.Remove(e); await db.SaveChangesAsync(ct); }
    }

    public async Task BulkUpsertResultsAsync(IEnumerable<ExamResult> results, CancellationToken ct)
    {
        foreach (var r in results)
        {
            var existing = await db.ExamResults.FirstOrDefaultAsync(
                x => x.ExamId == r.ExamId && x.StudentId == r.StudentId, ct);
            if (existing is null)
                db.ExamResults.Add(r);
            else
            {
                existing.MarksObtained = r.MarksObtained;
                existing.IsAbsent = r.IsAbsent;
                existing.Remarks = r.Remarks;
                db.ExamResults.Update(existing);
            }
        }
        await db.SaveChangesAsync(ct);
    }

    public async Task RecalculateRanksAsync(int examId, CancellationToken ct)
    {
        var results = await db.ExamResults
            .Where(r => r.ExamId == examId && !r.IsAbsent && r.MarksObtained.HasValue)
            .OrderByDescending(r => r.MarksObtained).ToListAsync(ct);

        var exam = await db.Exams.FindAsync([examId], ct);
        int rank = 1;
        foreach (var r in results)
        {
            r.Rank = rank++;
            if (exam is not null)
            {
                var pct = (double)r.MarksObtained!.Value / exam.TotalMarks * 100;
                r.Grade = pct switch { >= 90 => "A+", >= 80 => "A", >= 70 => "B+", >= 60 => "B", >= 50 => "C", _ => "D" };
            }
        }
        await db.SaveChangesAsync(ct);
    }
}
