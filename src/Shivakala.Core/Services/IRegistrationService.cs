using Shivakala.Core.ViewModels;

namespace Shivakala.Core.Services;

public interface IRegistrationService
{
    Task RegisterStudentAsync(RegistrationFormViewModel model, CancellationToken ct = default);
    Task RegisterAsync(RegistrationFormViewModel model, CancellationToken ct = default);
    Task<RegistrationFormViewModel> GetFormViewModelAsync(CancellationToken ct = default);
}
