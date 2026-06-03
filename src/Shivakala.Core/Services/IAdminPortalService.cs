using Shivakala.Core.ViewModels;

namespace Shivakala.Core.Services;

public interface IAdminPortalService
{
    Task<AdminDashboardViewModel> GetDashboardAsync(CancellationToken ct = default);
    Task<IReadOnlyList<StudentAdminViewModel>> GetRegistrationsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<EnquiryAdminViewModel>> GetEnquiriesAsync(CancellationToken ct = default);
    Task<int> CreateStudentAsync(AdminStudentFormViewModel model, CancellationToken ct = default);
    Task UpdateStudentStatusAsync(int id, string status, string? notes, CancellationToken ct = default);
    Task MarkEnquiryReadAsync(int id, string? reply, CancellationToken ct = default);
    Task<byte[]> ExportRegistrationsCsvAsync(CancellationToken ct = default);
    Task<byte[]> ExportEnquiriesCsvAsync(CancellationToken ct = default);
}
