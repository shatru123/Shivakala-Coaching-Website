namespace Shivakala.Core.ViewModels;

public sealed class AdminDashboardViewModel
{
    public int RegistrationCount { get; set; }

    public int EnquiryCount { get; set; }

    public IReadOnlyList<StudentAdminViewModel> RecentRegistrations { get; set; } = [];

    public IReadOnlyList<EnquiryAdminViewModel> RecentEnquiries { get; set; } = [];
}
