using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;
using Shivakala.Infrastructure.Data;

namespace Shivakala.Infrastructure.Repositories;

public sealed class TeacherRepository(ShivakalaDbContext db) : ITeacherRepository
{
    public Task<IReadOnlyList<Teacher>> GetAllAsync(CancellationToken ct)
        => db.Teachers.OrderBy(t => t.FullName).ToListAsync(ct)
             .ContinueWith(t => (IReadOnlyList<Teacher>)t.Result, ct);

    public Task<Teacher?> GetByIdAsync(int id, CancellationToken ct)
        => db.Teachers.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<Teacher> AddAsync(Teacher teacher, CancellationToken ct)
    {
        db.Teachers.Add(teacher);
        await db.SaveChangesAsync(ct);
        return teacher;
    }

    public async Task UpdateAsync(Teacher teacher, CancellationToken ct)
    {
        db.Teachers.Update(teacher);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var t = await db.Teachers.FindAsync([id], ct);
        if (t is not null) { db.Teachers.Remove(t); await db.SaveChangesAsync(ct); }
    }
}
