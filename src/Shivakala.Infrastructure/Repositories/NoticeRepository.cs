using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Entities;
using Shivakala.Infrastructure.Data;

namespace Shivakala.Infrastructure.Repositories;

public interface INoticeRepository
{
    Task<List<Notice>> GetActiveAsync(string? category = null, CancellationToken ct = default);
    Task<Notice?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Notice>> GetAllAdminAsync(CancellationToken ct = default);
    Task AddAsync(Notice notice, CancellationToken ct = default);
    Task UpdateAsync(Notice notice, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public sealed class NoticeRepository(ShivakalaDbContext db) : INoticeRepository
{
    public Task<List<Notice>> GetActiveAsync(string? category = null, CancellationToken ct = default)
    {
        var q = db.Notices.Where(n => n.IsActive);
        if (!string.IsNullOrWhiteSpace(category)) q = q.Where(n => n.Category == category);
        return q.OrderByDescending(n => n.IsPinned).ThenByDescending(n => n.PublishedDate).ToListAsync(ct);
    }
    public Task<Notice?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Notices.FirstOrDefaultAsync(n => n.Id == id, ct);
    public Task<List<Notice>> GetAllAdminAsync(CancellationToken ct = default) =>
        db.Notices.OrderByDescending(n => n.IsPinned).ThenByDescending(n => n.PublishedDate).ToListAsync(ct);
    public async Task AddAsync(Notice notice, CancellationToken ct = default) {
        db.Notices.Add(notice); await db.SaveChangesAsync(ct);
    }
    public async Task UpdateAsync(Notice notice, CancellationToken ct = default) {
        db.Notices.Update(notice); await db.SaveChangesAsync(ct);
    }
    public async Task DeleteAsync(int id, CancellationToken ct = default) {
        var n = await db.Notices.FindAsync([id], ct);
        if (n != null) { db.Notices.Remove(n); await db.SaveChangesAsync(ct); }
    }
}
