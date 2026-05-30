using Shivakala.Core.ViewModels;

namespace Shivakala.Core.Services;

public interface IRegistrationService
{
    Task RegisterStudentAsync(RegistrationFormViewModel model, CancellationToken cancellationToken = default);
}
