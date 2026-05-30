using Microsoft.Extensions.Options;
using Shivakala.Core.Services;

namespace Shivakala.Infrastructure.Services;

public sealed class AdminAuthenticationService(IOptions<AdminCredentialsOptions> options) : IAdminAuthenticationService
{
    private readonly AdminCredentialsOptions _credentials = options.Value;

    public bool ValidateCredentials(string username, string password)
        => string.Equals(username?.Trim(), _credentials.Username, StringComparison.Ordinal)
           && string.Equals(password, _credentials.Password, StringComparison.Ordinal);
}
