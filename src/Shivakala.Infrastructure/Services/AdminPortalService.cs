using Shivakala.Core.Interfaces;
using Shivakala.Core.Services;
using Shivakala.Core.ViewModels;

namespace Shivakala.Infrastructure.Services;

public sealed class AdminPortalService(
    IStudentRepository studentRepository,
    IEnquiryRepository enquiryRepository) : IAdminPortalService
{
    public async Task<AdminDashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var registrations = await GetRegistrationsAsync(cancellationToken);
        var enquiries = await GetEnquiriesAsync(cancellationToken);

        return new AdminDashboardViewModel
        {
            RegistrationCount = registrations.Count,
            EnquiryCount = enquiries.Count,
            RecentRegistrations = registrations.Take(5).ToList(),
            RecentEnquiries = enquiries.Take(5).ToList()
        };
    }

    public async Task<IReadOnlyList<StudentAdminViewModel>> GetRegistrationsAsync(CancellationToken cancellationToken = default)
    {
        var students = await studentRepository.ListRecentAsync(cancellationToken);
        return students.Select(x => new StudentAdminViewModel
        {
            Id = x.Id,
            FullName = x.FullName,
            Mobile = x.Mobile,
            Email = x.Email,
            Standard = x.Standard,
            Subject = x.Subject,
            Address = x.Address,
            CreatedDate = x.CreatedDate
        }).ToList();
    }

    public async Task<IReadOnlyList<EnquiryAdminViewModel>> GetEnquiriesAsync(CancellationToken cancellationToken = default)
    {
        var enquiries = await enquiryRepository.ListRecentAsync(cancellationToken);
        return enquiries.Select(x => new EnquiryAdminViewModel
        {
            Id = x.Id,
            Name = x.Name,
            Mobile = x.Mobile,
            Message = x.Message,
            CreatedDate = x.CreatedDate
        }).ToList();
    }
}
