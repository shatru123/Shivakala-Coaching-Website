using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;
using Shivakala.Infrastructure.Data;

namespace Shivakala.Infrastructure.Repositories;

public sealed class BatchRepository(ShivakalaDbContext db) : IBatchRepository
{
    public async Task<IReadOnlyList<Batch>> GetAllAsync(CancellationToken ct)
    {
        if (await TeacherSchemaCompatibility.SupportsAboutPageFieldsAsync(db, ct))
            return await db.Batches.Include(b => b.BatchSubjects).ThenInclude(bs => bs.Teacher)
                .OrderBy(b => b.Standard).ThenBy(b => b.Name)
                .ToListAsync(ct);

        var batches = await db.Batches.Include(b => b.BatchSubjects)
            .OrderBy(b => b.Standard).ThenBy(b => b.Name)
            .ToListAsync(ct);
        var teacherNames = await TeacherSchemaCompatibility.GetTeacherNamesFallbackAsync(db, ct);
        AttachTeacherNames(batches, teacherNames);
        return batches;
    }

    public async Task<Batch?> GetByIdWithDetailsAsync(int id, CancellationToken ct)
    {
        if (await TeacherSchemaCompatibility.SupportsAboutPageFieldsAsync(db, ct))
            return await db.Batches.Include(b => b.BatchSubjects).ThenInclude(bs => bs.Teacher)
                .Include(b => b.StudentBatches).ThenInclude(sb => sb.Student)
                .Include(b => b.TimetableSlots).ThenInclude(ts => ts.Teacher)
                .FirstOrDefaultAsync(b => b.Id == id, ct);

        var batch = await db.Batches.Include(b => b.BatchSubjects)
            .Include(b => b.StudentBatches).ThenInclude(sb => sb.Student)
            .Include(b => b.TimetableSlots)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
        if (batch is null)
            return null;

        var teacherNames = await TeacherSchemaCompatibility.GetTeacherNamesFallbackAsync(db, ct);
        AttachTeacherNames([batch], teacherNames);
        return batch;
    }

    public async Task<Batch> AddAsync(Batch batch, CancellationToken ct)
    {
        db.Batches.Add(batch);
        await db.SaveChangesAsync(ct);
        return batch;
    }

    public async Task UpdateAsync(Batch batch, CancellationToken ct)
    {
        db.Batches.Update(batch);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var b = await db.Batches.FindAsync([id], ct);
        if (b is not null) { db.Batches.Remove(b); await db.SaveChangesAsync(ct); }
    }

    public Task<int> GetStudentCountAsync(int batchId, CancellationToken ct)
        => db.StudentBatches.CountAsync(sb => sb.BatchId == batchId && sb.IsActive, ct);

    private static void AttachTeacherNames(IEnumerable<Batch> batches, IReadOnlyDictionary<int, string> teacherNames)
    {
        foreach (var batch in batches)
        {
            foreach (var subject in batch.BatchSubjects.Where(bs => bs.TeacherId.HasValue))
            {
                if (subject.TeacherId.HasValue && teacherNames.TryGetValue(subject.TeacherId.Value, out var teacherName))
                    subject.Teacher = new Teacher { Id = subject.TeacherId.Value, FullName = teacherName, Mobile = string.Empty };
            }

            foreach (var slot in batch.TimetableSlots.Where(ts => ts.TeacherId.HasValue))
            {
                if (slot.TeacherId.HasValue && teacherNames.TryGetValue(slot.TeacherId.Value, out var teacherName))
                    slot.Teacher = new Teacher { Id = slot.TeacherId.Value, FullName = teacherName, Mobile = string.Empty };
            }
        }
    }
}
