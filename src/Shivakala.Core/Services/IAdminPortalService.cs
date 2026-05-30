using Shivakala.Core.ViewModels;

namespace Shivakala.Core.Services;

public interface IAdminPortalService
{
    Task<AdminDashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentAdminViewModel>> GetRegistrationsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EnquiryAdminViewModel>> GetEnquiriesAsync(CancellationToken cancellationToken = default);
}
