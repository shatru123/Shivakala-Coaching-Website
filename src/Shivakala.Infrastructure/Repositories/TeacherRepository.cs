using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;
using Shivakala.Infrastructure.Data;

namespace Shivakala.Infrastructure.Repositories;

public sealed class TeacherRepository(ShivakalaDbContext db) : ITeacherRepository
{
    public async Task<IReadOnlyList<Teacher>> GetAllAsync(CancellationToken ct)
    {
        if (!await TeacherSchemaCompatibility.SupportsAboutPageFieldsAsync(db, ct))
            return await TeacherSchemaCompatibility.GetTeachersFallbackAsync(db, ct);

        return await db.Teachers.OrderBy(t => t.FullName).ToListAsync(ct);
    }

    public async Task<Teacher?> GetByIdAsync(int id, CancellationToken ct)
    {
        if (!await TeacherSchemaCompatibility.SupportsAboutPageFieldsAsync(db, ct))
            return await TeacherSchemaCompatibility.GetTeacherFallbackAsync(db, id, ct);

        return await db.Teachers.FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<Teacher> AddAsync(Teacher teacher, CancellationToken ct)
    {
        if (!await TeacherSchemaCompatibility.SupportsAboutPageFieldsAsync(db, ct))
        {
            await TeacherSchemaCompatibility.InsertTeacherFallbackAsync(db, teacher, ct);
            return teacher;
        }

        db.Teachers.Add(teacher);
        await db.SaveChangesAsync(ct);
        return teacher;
    }

    public async Task UpdateAsync(Teacher teacher, CancellationToken ct)
    {
        if (!await TeacherSchemaCompatibility.SupportsAboutPageFieldsAsync(db, ct))
        {
            await TeacherSchemaCompatibility.UpdateTeacherFallbackAsync(db, teacher, ct);
            return;
        }

        db.Teachers.Update(teacher);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        if (!await TeacherSchemaCompatibility.SupportsAboutPageFieldsAsync(db, ct))
        {
            await TeacherSchemaCompatibility.DeleteTeacherFallbackAsync(db, id, ct);
            return;
        }

        var t = await db.Teachers.FindAsync([id], ct);
        if (t is not null) { db.Teachers.Remove(t); await db.SaveChangesAsync(ct); }
    }
}
