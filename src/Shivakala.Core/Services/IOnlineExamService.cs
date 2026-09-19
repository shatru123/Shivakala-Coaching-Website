using Shivakala.Core.Entities;
using Shivakala.Core.ViewModels;

namespace Shivakala.Core.Services;

public interface IOnlineExamService
{
    // ── Admin / Teacher Wizard & Exam Setup ─────────────────────────────────
    Task<OnlineExamWizardViewModel?> GetWizardViewModelAsync(int? examId, CancellationToken ct = default);
    Task<int> SaveOnlineExamWizardAsync(OnlineExamWizardViewModel model, CancellationToken ct = default);
    Task TogglePublishAsync(int examId, CancellationToken ct = default);

    // ── Question Bank ────────────────────────────────────────────────────────
    Task<IReadOnlyList<Question>> SearchQuestionBankAsync(
        string? search = null,
        string? subject = null,
        string? standard = null,
        string? topic = null,
        string? difficulty = null,
        CancellationToken ct = default);

    Task<Question> AddQuestionToBankAsync(Question question, CancellationToken ct = default);
    Task<bool> AddQuestionsToExamAsync(int examId, IEnumerable<int> questionIds, CancellationToken ct = default);

    // ── Student Portal & Exam Engine ─────────────────────────────────────────
    Task<IReadOnlyList<OnlineExamStudentListViewModel>> GetStudentExamsAsync(int studentId, CancellationToken ct = default);
    Task<bool> RegisterStudentForExamAsync(int examId, int studentId, CancellationToken ct = default);

    Task<(bool Success, string Message, OnlineExamAttemptViewModel? Attempt)> StartOrResumeAttemptAsync(
        int examId, int studentId, CancellationToken ct = default);

    Task<(bool Success, string Message)> AutoSaveAnswerAsync(
        SaveAnswerRequestModel model, int studentId, CancellationToken ct = default);

    Task<(bool Success, string Message, int? AttemptId)> SubmitAttemptAsync(
        int attemptId, int studentId, bool isAutoSubmit = false, CancellationToken ct = default);

    // ── MPSC Candidate Exam Engine ───────────────────────────────────────────
    Task<(bool Success, string Message, OnlineExamAttemptViewModel? Attempt)> StartOrResumeMpscAttemptAsync(
        int examId, int mpscStudentId, CancellationToken ct = default);

    Task<(bool Success, string Message)> AutoSaveMpscAnswerAsync(
        SaveAnswerRequestModel model, int mpscStudentId, CancellationToken ct = default);

    Task<(bool Success, string Message, int? AttemptId)> SubmitMpscAttemptAsync(
        int attemptId, int mpscStudentId, bool isAutoSubmit = false, CancellationToken ct = default);

    Task<OnlineExamResultViewModel?> GetMpscAttemptResultAsync(
        int attemptId, int mpscStudentId, CancellationToken ct = default);

    // ── Results & Analytics ──────────────────────────────────────────────────
    Task<OnlineExamResultViewModel?> GetAttemptResultAsync(
        int attemptId, int currentUserId, bool isStudent = true, CancellationToken ct = default);

    Task<OnlineExamAnalyticsViewModel?> GetExamAnalyticsAsync(int examId, CancellationToken ct = default);
    Task<byte[]> ExportResultsToCsvAsync(int examId, CancellationToken ct = default);
}
