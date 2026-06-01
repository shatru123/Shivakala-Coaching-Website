using Shivakala.Core.Entities;
using Shivakala.Core.Services;
using Shivakala.Infrastructure.Data;

namespace Shivakala.Infrastructure.Services;

public sealed class AuditService(ShivakalaDbContext db) : IAuditService
{
    public async Task LogAsync(string action, string entityType, int? entityId,
                               string? oldValues, string? newValues,
                               string? username, string? ip,
                               CancellationToken ct = default)
    {
        db.AuditLogs.Add(new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            PerformedByUsername = username,
            IpAddress = ip,
            CreatedDate = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
    }
}
