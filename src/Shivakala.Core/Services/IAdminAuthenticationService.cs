namespace Shivakala.Core.Services;

public interface IAdminAuthenticationService
{
    bool ValidateCredentials(string username, string password);
}
