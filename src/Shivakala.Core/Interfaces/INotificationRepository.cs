using Shivakala.Core.Entities;

namespace Shivakala.Core.Interfaces;

public interface INotificationRepository
{
    Task<IReadOnlyList<Notification>> GetAllAsync(int take = 50, CancellationToken ct = default);
    Task<Notification> AddAsync(Notification notification, CancellationToken ct = default);
    Task UpdateAsync(Notification notification, CancellationToken ct = default);
}
