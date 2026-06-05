using Shivakala.Core.Entities;

namespace Shivakala.Core.Services;

public interface IAdminAuthenticationService
{
    Task<AppUser?> ValidateCredentialsAsync(string username, string password, CancellationToken ct = default);
}
