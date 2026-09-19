using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Shivakala.Core.Entities;

namespace Shivakala.Core.ViewModels;

// ── Public & Candidate ViewModels ─────────────────────────────────────────

public sealed class MpscRegistrationViewModel
{
    // Personal Details
    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(150, ErrorMessage = "Full Name cannot exceed 150 characters.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Full Name in Marathi")]
    [StringLength(150)]
    public string? FullNameMarathi { get; set; }

    [Required(ErrorMessage = "Date of Birth is required.")]
    [Display(Name = "Date of Birth")]
    public string DateOfBirth { get; set; } = string.Empty;

    [Required(ErrorMessage = "Gender is required.")]
    [Display(Name = "Gender")]
    public string Gender { get; set; } = "Male";

    [Required(ErrorMessage = "Mobile number is required.")]
    [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Please enter a valid 10-digit Indian mobile number.")]
    [Display(Name = "Mobile Number")]
    public string Mobile { get; set; } = string.Empty;

    [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Please enter a valid 10-digit mobile number.")]
    [Display(Name = "Alternate Mobile")]
    public string? AlternateMobile { get; set; }

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Profile Photo")]
    public IFormFile? PhotoFile { get; set; }

    // Address
    [Required(ErrorMessage = "Address is required.")]
    [Display(Name = "Address")]
    public string Address { get; set; } = string.Empty;

    [Display(Name = "City / Village")]
    public string? City { get; set; }

    [Required(ErrorMessage = "District is required.")]
    [Display(Name = "District")]
    public string District { get; set; } = string.Empty;

    [Display(Name = "State")]
    public string State { get; set; } = "Maharashtra";

    [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must be 6 digits.")]
    [Display(Name = "Pincode")]
    public string? Pincode { get; set; }

    // Education
    [Required(ErrorMessage = "Highest Qualification is required.")]
    [Display(Name = "Highest Qualification")]
    public string HighestQualification { get; set; } = "Graduate";

    [Display(Name = "Degree / Course Name")]
    public string? DegreeOrCourse { get; set; }

    [Display(Name = "University / Board")]
    public string? University { get; set; }

    [Range(1980, 2035, ErrorMessage = "Enter a valid year between 1980 and 2035.")]
    [Display(Name = "Year of Passing")]
    public int? YearOfPassing { get; set; }

    // MPSC Preferences
    [Required(ErrorMessage = "Preferred examination is required.")]
    [Display(Name = "Target Examination")]
    public string PreferredExam { get; set; } = "Rajyaseva";

    [Display(Name = "Other Examination Name")]
    public string? OtherExamInterest { get; set; }

    [Display(Name = "Preparation Level")]
    public string PreparationLevel { get; set; } = "Beginner";

    [Display(Name = "Target Attempt Year")]
    [Range(2024, 2032, ErrorMessage = "Enter a valid target year.")]
    public int? TargetAttemptYear { get; set; } = DateTime.UtcNow.Year;

    [Display(Name = "Appeared previously?")]
    public bool PreviousAttempts { get; set; } = false;

    [Display(Name = "Number of previous attempts")]
    [Range(0, 20)]
    public int? NumberOfPreviousAttempts { get; set; } = 0;

    // Category
    [Display(Name = "Social Category")]
    public string Category { get; set; } = "Open";

    [Display(Name = "Caste Certificate Available")]
    public bool CasteCertificateAvailable { get; set; } = false;

    [Display(Name = "Non-Creamy Layer Certificate Available")]
    public bool NonCreamyLayerCertificateAvailable { get; set; } = false;

    [Display(Name = "EWS Certificate Available")]
    public bool EwsCertificateAvailable { get; set; } = false;

    // Password
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "Create Password")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    // Consent
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to Shivakala's terms and privacy policy.")]
    [Display(Name = "I agree to Shivakala's terms and privacy policy")]
    public bool AgreeTerms { get; set; } = false;

    [Range(typeof(bool), "true", "true", ErrorMessage = "You must confirm that the provided information is correct.")]
    [Display(Name = "I confirm that all information provided is accurate and true")]
    public bool ConfirmAccuracy { get; set; } = false;
}

public sealed class MpscLoginViewModel
{
    [Required(ErrorMessage = "Registration Number, Mobile, or Email is required.")]
    [Display(Name = "Registration No / Mobile / Email")]
    public string Identifier { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember Me")]
    public bool RememberMe { get; set; } = true;

    public string? ReturnUrl { get; set; }
}

public sealed class MpscCandidateDashboardViewModel
{
    public MpscStudent Candidate { get; set; } = null!;
    public int ProfileCompletionPercentage { get; set; }
    public List<string> MissingFields { get; set; } = [];

    // Dashboard Cards
    public int AvailableExamsCount { get; set; }
    public int RegisteredExamsCount { get; set; }
    public int UpcomingExamsCount { get; set; }
    public int CompletedExamsCount { get; set; }
    public int ResultsCount { get; set; }

    public List<MpscExamCardViewModel> AvailableExams { get; set; } = [];
    public List<MpscResultHistoryItemViewModel> RecentResults { get; set; } = [];
}

public sealed class MpscProfileViewModel
{
    public int Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Full Name in Marathi")]
    public string? FullNameMarathi { get; set; }

    [Display(Name = "Date of Birth")]
    public string DateOfBirth { get; set; } = string.Empty;

    [Display(Name = "Gender")]
    public string Gender { get; set; } = "Male";

    [Required, RegularExpression(@"^[6-9]\d{9}$")]
    [Display(Name = "Mobile")]
    public string Mobile { get; set; } = string.Empty;

    [RegularExpression(@"^[6-9]\d{9}$")]
    [Display(Name = "Alternate Mobile")]
    public string? AlternateMobile { get; set; }

    [Required, EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    public string? CurrentPhotoUrl { get; set; }
    public IFormFile? NewPhotoFile { get; set; }

    [Required]
    [Display(Name = "Address")]
    public string Address { get; set; } = string.Empty;

    [Display(Name = "City")]
    public string? City { get; set; }

    [Required]
    [Display(Name = "District")]
    public string District { get; set; } = string.Empty;

    [Display(Name = "State")]
    public string State { get; set; } = "Maharashtra";

    [Display(Name = "Pincode")]
    public string? Pincode { get; set; }

    [Required]
    [Display(Name = "Highest Qualification")]
    public string HighestQualification { get; set; } = "Graduate";

    [Display(Name = "Degree / Course")]
    public string? DegreeOrCourse { get; set; }

    [Display(Name = "University")]
    public string? University { get; set; }

    [Display(Name = "Year of Passing")]
    public int? YearOfPassing { get; set; }

    [Display(Name = "Target Exam")]
    public string PreferredExam { get; set; } = "Rajyaseva";

    [Display(Name = "Other Exam")]
    public string? OtherExamInterest { get; set; }

    [Display(Name = "Preparation Level")]
    public string PreparationLevel { get; set; } = "Beginner";

    [Display(Name = "Target Year")]
    public int? TargetAttemptYear { get; set; }

    [Display(Name = "Previous Attempts")]
    public bool PreviousAttempts { get; set; }

    [Display(Name = "Attempt Count")]
    public int? NumberOfPreviousAttempts { get; set; }

    [Display(Name = "Category")]
    public string Category { get; set; } = "Open";

    [Display(Name = "Caste Certificate")]
    public bool CasteCertificateAvailable { get; set; }

    [Display(Name = "Non-Creamy Layer")]
    public bool NonCreamyLayerCertificateAvailable { get; set; }

    [Display(Name = "EWS Certificate")]
    public bool EwsCertificateAvailable { get; set; }
}

public sealed class MpscExamCardViewModel
{
    public int ExamId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string ExamMode { get; set; } = "Online"; // Online | Offline
    public string? Venue { get; set; }
    public int? Capacity { get; set; }
    public int RegisteredCount { get; set; }
    public int TotalMarks { get; set; }
    public int PassingMarks { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime ExamDate { get; set; }
    public DateTime? StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public bool IsRegistered { get; set; }
    public bool CanAttempt { get; set; }
    public bool HasAttempted { get; set; }
    public int? ResultId { get; set; }
}

public sealed class MpscResultHistoryItemViewModel
{
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Mode { get; set; } = "Online";
    public DateTime Date { get; set; }
    public double? MarksObtained { get; set; }
    public int TotalMarks { get; set; }
    public int PassingMarks { get; set; }
    public int? Rank { get; set; }
    public string? Grade { get; set; }
    public bool IsAbsent { get; set; }
    public string? Remarks { get; set; }
    public int? ResultId { get; set; }
    public int? OnlineAttemptId { get; set; }
    public bool IsPassed => (MarksObtained ?? 0) >= PassingMarks;
}

public sealed class MpscChangePasswordViewModel
{
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm New Password")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

// ── Admin MPSC ViewModels ──────────────────────────────────────────────────

public sealed class MpscAdminDashboardViewModel
{
    public int TotalCandidates { get; set; }
    public int ActiveCandidates { get; set; }
    public int InactiveCandidates { get; set; }
    public int NewRegistrationsToday { get; set; }
    public int NewRegistrationsThisMonth { get; set; }
    public int OnlineExamRegistrations { get; set; }
    public int OfflineExamRegistrations { get; set; }
    public int UpcomingMpscExamsCount { get; set; }
    public int CompletedMpscExamsCount { get; set; }
    public int PendingResultsCount { get; set; }

    public List<MpscStudent> RecentCandidates { get; set; } = [];
    public List<MpscExamCardViewModel> UpcomingExams { get; set; } = [];
}

public sealed class MpscCandidateListFilterViewModel
{
    public string? Search { get; set; }
    public string? Status { get; set; } // Active | Inactive
    public string? ExamInterest { get; set; }
    public string? District { get; set; }
    public int? TargetYear { get; set; }
    public string? PreparationLevel { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public int TotalRecords { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / Math.Max(1, PageSize));

    public List<MpscStudent> Candidates { get; set; } = [];
    public List<string> AvailableDistricts { get; set; } = [];
}

public sealed class MpscCandidateDetailViewModel
{
    public MpscStudent Candidate { get; set; } = null!;
    public AppUser? UserAccount { get; set; }
    public List<MpscExamRegistration> Registrations { get; set; } = [];
    public List<OnlineExamAttempt> OnlineAttempts { get; set; } = [];
    public List<MpscExamResult> Results { get; set; } = [];
}

public sealed class MpscImportPreviewViewModel
{
    public string FileName { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public int ValidCount => ValidRows.Count;
    public int DuplicateCount => DuplicateRows.Count;
    public int InvalidCount => InvalidRows.Count;

    public List<MpscImportRow> ValidRows { get; set; } = [];
    public List<MpscImportRow> DuplicateRows { get; set; } = [];
    public List<MpscImportRow> InvalidRows { get; set; } = [];
}

public sealed class MpscImportRow
{
    public int RowNumber { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public string ExamInterest { get; set; } = string.Empty;
    public int? TargetYear { get; set; }
    public string Status { get; set; } = "Valid"; // Valid | Duplicate | Invalid
    public string? Reason { get; set; }
}

public sealed class OfflineCheckInViewModel
{
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; }
    public string? Venue { get; set; }
    public int? Capacity { get; set; }
    public int RegisteredCount { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public string? SearchTerm { get; set; }

    public List<OfflineParticipantCheckInItem> Participants { get; set; } = [];
}

public sealed class OfflineParticipantCheckInItem
{
    public int RegistrationId { get; set; }
    public int MpscStudentId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string? SeatNumber { get; set; }
    public string AttendanceStatus { get; set; } = "Pending"; // Pending | Present | Absent
    public DateTime? AttendanceMarkedAt { get; set; }
    public string? Remarks { get; set; }
}

public sealed class OfflineResultEntryViewModel
{
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public int TotalMarks { get; set; }
    public int PassingMarks { get; set; }
    public DateTime ExamDate { get; set; }

    public List<OfflineResultEntryRow> Rows { get; set; } = [];
}

public sealed class OfflineResultEntryRow
{
    public int MpscStudentId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? SeatNumber { get; set; }
    public double? MarksObtained { get; set; }
    public int? Rank { get; set; }
    public string? Grade { get; set; }
    public bool IsAbsent { get; set; }
    public string? Remarks { get; set; }
}
