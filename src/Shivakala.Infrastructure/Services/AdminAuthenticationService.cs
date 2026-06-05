using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shivakala.Core.Entities;
using Shivakala.Core.Services;
using Shivakala.Infrastructure.Data;
using Shivakala.Infrastructure.Security;

namespace Shivakala.Infrastructure.Services;

public sealed class AdminAuthenticationService(
    ShivakalaDbContext db,
    IOptions<AdminCredentialsOptions> options) : IAdminAuthenticationService
{
    private readonly AdminCredentialsOptions _credentials = options.Value;

    public async Task<AppUser?> ValidateCredentialsAsync(string username, string password, CancellationToken ct = default)
    {
        var normalizedUsername = username?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedUsername) || string.IsNullOrWhiteSpace(password))
            return null;

        var adminUser = await db.AppUsers.FirstOrDefaultAsync(
            u => u.Role == "Admin" && u.IsActive && u.Username.ToLower() == normalizedUsername.ToLower(), ct);

        if (adminUser is not null)
            return PasswordHasher.Verify(password, adminUser.PasswordHash) ? adminUser : null;

        if (!string.Equals(normalizedUsername, _credentials.Username, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(password, _credentials.Password, StringComparison.Ordinal))
        {
            return null;
        }

        var seededAdmin = new AppUser
        {
            Username = normalizedUsername,
            Email = $"{normalizedUsername}@portal.shivakala.local",
            PasswordHash = PasswordHasher.Hash(password),
            Role = "Admin",
            FullName = "Administrator",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        db.AppUsers.Add(seededAdmin);
        await db.SaveChangesAsync(ct);
        return seededAdmin;
    }
}
