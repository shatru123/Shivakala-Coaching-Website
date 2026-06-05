using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;
using Shivakala.Infrastructure.Data;

namespace Shivakala.Infrastructure.Repositories;

public sealed class BatchRepository(ShivakalaDbContext db) : IBatchRepository
{
    public Task<IReadOnlyList<Batch>> GetAllAsync(CancellationToken ct)
        => db.Batches.Include(b => b.BatchSubjects).ThenInclude(bs => bs.Teacher)
                     .OrderBy(b => b.Standard).ThenBy(b => b.Name)
                     .ToListAsync(ct).ContinueWith(t => (IReadOnlyList<Batch>)t.Result, ct);

    public Task<Batch?> GetByIdWithDetailsAsync(int id, CancellationToken ct)
        => db.Batches.Include(b => b.BatchSubjects).ThenInclude(bs => bs.Teacher)
                     .Include(b => b.StudentBatches).ThenInclude(sb => sb.Student)
                     .Include(b => b.TimetableSlots).ThenInclude(ts => ts.Teacher)
                     .FirstOrDefaultAsync(b => b.Id == id, ct);

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
}
