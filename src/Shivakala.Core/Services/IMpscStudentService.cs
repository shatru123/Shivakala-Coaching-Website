using Shivakala.Core.Entities;
using Shivakala.Core.ViewModels;

namespace Shivakala.Core.Services;

public interface IMpscStudentService
{
    // Registration & Auth
    Task<(bool Success, string Message, MpscStudent? Candidate, string? GeneratedPassword)> RegisterCandidateAsync(
        MpscRegistrationViewModel model, string? ipAddress = null, CancellationToken ct = default);
    Task<MpscStudent?> GetCandidateByIdAsync(int id, CancellationToken ct = default);
    Task<MpscStudent?> GetCandidateByUserIdAsync(int appUserId, CancellationToken ct = default);
    Task<MpscCandidateDashboardViewModel> GetDashboardAsync(int mpscStudentId, CancellationToken ct = default);
    Task<MpscProfileViewModel?> GetProfileAsync(int mpscStudentId, CancellationToken ct = default);
    Task<(bool Success, string Message)> UpdateProfileAsync(MpscProfileViewModel model, CancellationToken ct = default);
    Task<(bool Success, string Message)> ChangePasswordAsync(int mpscStudentId, string currentPassword, string newPassword, CancellationToken ct = default);

    // Exam Registration & Results for Candidates
    Task<IReadOnlyList<MpscExamCardViewModel>> GetAvailableExamsAsync(int mpscStudentId, CancellationToken ct = default);
    Task<(bool Success, string Message)> RegisterForExamAsync(int examId, int mpscStudentId, CancellationToken ct = default);
    Task<IReadOnlyList<MpscResultHistoryItemViewModel>> GetCandidateResultsAsync(int mpscStudentId, CancellationToken ct = default);

    // Admin Operations
    Task<MpscAdminDashboardViewModel> GetAdminDashboardAsync(CancellationToken ct = default);
    Task<MpscCandidateListFilterViewModel> GetCandidatesAsync(MpscCandidateListFilterViewModel filter, CancellationToken ct = default);
    Task<MpscCandidateDetailViewModel?> GetCandidateDetailAsync(int candidateId, CancellationToken ct = default);
    Task<(bool Success, string Message)> ToggleCandidateStatusAsync(int candidateId, CancellationToken ct = default);
    Task<(bool Success, string Message)> ResetCandidatePasswordAsync(int candidateId, string newPassword, CancellationToken ct = default);
    Task<byte[]> ExportCandidatesCsvAsync(MpscCandidateListFilterViewModel filter, CancellationToken ct = default);
    Task<MpscImportPreviewViewModel> PreviewImportCsvAsync(Stream csvStream, CancellationToken ct = default);
    Task<(int ImportedCount, int SkippedCount, List<string> Errors)> CommitImportAsync(List<MpscImportRow> rows, CancellationToken ct = default);

    // Offline Exam Check-In & Marks Entry
    Task<IReadOnlyList<Exam>> GetMpscExamsForAdminAsync(CancellationToken ct = default);
    Task<OfflineCheckInViewModel?> GetOfflineCheckInListAsync(int examId, string? search = null, CancellationToken ct = default);
    Task<(bool Success, string Message)> UpdateOfflineAttendanceAsync(int registrationId, string status, string? seatNumber, string? remarks, CancellationToken ct = default);
    Task<OfflineResultEntryViewModel?> GetOfflineResultEntryModelAsync(int examId, CancellationToken ct = default);
    Task<(bool Success, string Message)> SaveOfflineResultsAsync(OfflineResultEntryViewModel model, CancellationToken ct = default);
}
