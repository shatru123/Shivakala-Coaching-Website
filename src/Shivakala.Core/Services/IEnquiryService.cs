using Shivakala.Core.ViewModels;

namespace Shivakala.Core.Services;

public interface IEnquiryService
{
    Task SubmitEnquiryAsync(EnquiryFormViewModel model, CancellationToken cancellationToken = default);
}
