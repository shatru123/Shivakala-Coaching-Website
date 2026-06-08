using Shivakala.Core.ViewModels;

namespace Shivakala.Core.Services;

public interface IAboutPageService
{
    Task<AboutPageViewModel> GetAboutPageAsync(CancellationToken cancellationToken = default);
}
