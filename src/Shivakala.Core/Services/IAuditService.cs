namespace Shivakala.Core.Services;

public interface IAuditService
{
    Task LogAsync(string action, string entityType, int? entityId,
                  string? oldValues, string? newValues,
                  string? username, string? ip,
                  CancellationToken ct = default);
}
