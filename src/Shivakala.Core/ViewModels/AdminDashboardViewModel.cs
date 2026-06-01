namespace Shivakala.Core.ViewModels;

public sealed class AdminDashboardViewModel
{
    public int RegistrationCount { get; set; }
    public int EnquiryCount { get; set; }
    public int UnreadEnquiries { get; set; }
    public int PendingAdmissions { get; set; }
    public int NoticeCount { get; set; }
    public int MaterialCount { get; set; }
    public IReadOnlyList<StudentAdminViewModel> RecentRegistrations { get; set; } = [];
    public IReadOnlyList<EnquiryAdminViewModel> RecentEnquiries { get; set; } = [];

    // alias for compat
    public int TotalRegistrations => RegistrationCount;
    public int TotalEnquiries => EnquiryCount;
    public int NewRegistrationsThisMonth { get; set; }
    public int NewEnquiriesThisMonth { get; set; }
}
