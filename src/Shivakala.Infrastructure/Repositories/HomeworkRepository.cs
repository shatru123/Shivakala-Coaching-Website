using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;
using Shivakala.Infrastructure.Data;

namespace Shivakala.Infrastructure.Repositories;

public sealed class HomeworkRepository(ShivakalaDbContext db) : IHomeworkRepository
{
    public Task<IReadOnlyList<Homework>> GetAllAsync(string? standard, string? subject, CancellationToken ct)
    {
        var q = db.Homeworks.Include(h => h.AssignedByTeacher).Include(h => h.Batch).AsQueryable();
        if (!string.IsNullOrWhiteSpace(standard)) q = q.Where(h => h.Standard == standard);
        if (!string.IsNullOrWhiteSpace(subject))  q = q.Where(h => h.Subject == subject);
        return q.OrderByDescending(h => h.CreatedDate).ToListAsync(ct)
                .ContinueWith(t => (IReadOnlyList<Homework>)t.Result, ct);
    }

    public Task<Homework?> GetByIdAsync(int id, CancellationToken ct)
        => db.Homeworks.Include(h => h.AssignedByTeacher).Include(h => h.Batch)
             .FirstOrDefaultAsync(h => h.Id == id, ct);

    public async Task<Homework> AddAsync(Homework h, CancellationToken ct)
    { db.Homeworks.Add(h); await db.SaveChangesAsync(ct); return h; }

    public async Task UpdateAsync(Homework h, CancellationToken ct)
    { db.Homeworks.Update(h); await db.SaveChangesAsync(ct); }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var h = await db.Homeworks.FindAsync([id], ct);
        if (h is not null) { db.Homeworks.Remove(h); await db.SaveChangesAsync(ct); }
    }

    public Task<IReadOnlyList<HomeworkSubmission>> GetSubmissionsAsync(int homeworkId, CancellationToken ct)
        => db.HomeworkSubmissions.Include(s => s.Student).Where(s => s.HomeworkId == homeworkId)
             .ToListAsync(ct).ContinueWith(t => (IReadOnlyList<HomeworkSubmission>)t.Result, ct);

    public async Task AddSubmissionAsync(HomeworkSubmission s, CancellationToken ct)
    { db.HomeworkSubmissions.Add(s); await db.SaveChangesAsync(ct); }
}
