using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;
using Shivakala.Infrastructure.Data;

namespace Shivakala.Infrastructure.Repositories;

public sealed class NotificationRepository(ShivakalaDbContext db) : INotificationRepository
{
    public Task<IReadOnlyList<Notification>> GetAllAsync(int take, CancellationToken ct)
        => db.Notifications.OrderByDescending(n => n.CreatedDate).Take(take).ToListAsync(ct)
             .ContinueWith(t => (IReadOnlyList<Notification>)t.Result, ct);

    public async Task<Notification> AddAsync(Notification n, CancellationToken ct)
    { db.Notifications.Add(n); await db.SaveChangesAsync(ct); return n; }

    public async Task UpdateAsync(Notification n, CancellationToken ct)
    { db.Notifications.Update(n); await db.SaveChangesAsync(ct); }
}
