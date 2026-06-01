using Shivakala.Core.ViewModels;

namespace Shivakala.Core.Services;

public interface IEnquiryService
{
    Task SubmitEnquiryAsync(EnquiryFormViewModel model, CancellationToken ct = default);
    Task<EnquiryFormViewModel> GetFormViewModelAsync(CancellationToken ct = default);
}
