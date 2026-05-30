using Shivakala.Core.ViewModels;

namespace Shivakala.Core.Services;

public interface IHomePageService
{
    Task<HomePageViewModel> GetHomePageAsync(CancellationToken cancellationToken = default);
}
