namespace Shivakala.Core.ViewModels;

public sealed class QuestionImportPreviewViewModel
{
    public string FileName { get; set; } = string.Empty;
    public int TotalDetected { get; set; }
    public int ValidCount { get; set; }
    public int NeedsReviewCount { get; set; }
    public int InvalidCount { get; set; }
    public List<ImportedQuestionItem> Questions { get; set; } = [];
}

public sealed class ImportedQuestionItem
{
    public string TempId { get; set; } = Guid.NewGuid().ToString("N");
    public int QuestionNumber { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? QuestionImageUrl { get; set; }
    public string? Topic { get; set; }
    public string Difficulty { get; set; } = "Medium";
    public int Marks { get; set; } = 1;
    public double NegativeMarks { get; set; } = 0.0;
    public string? Explanation { get; set; }
    public List<ImportedQuestionOptionItem> Options { get; set; } = [];
    public string? CorrectOptionKey { get; set; }
    public string ValidationStatus { get; set; } = "Valid"; // Valid | NeedsReview | Invalid
    public string? ValidationMessage { get; set; }
}

public sealed class ImportedQuestionOptionItem
{
    public string OptionKey { get; set; } = "A";
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

public sealed class OnlineExamWizardViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Standard { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public int? BatchId { get; set; }
    public string ExamType { get; set; } = "Weekly";
    public string? Description { get; set; }
    public string? Instructions { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public DateTime? RegistrationOpenDate { get; set; }
    public DateTime? RegistrationCloseDate { get; set; }
    public DateTime? StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public int PassingMarks { get; set; } = 35;
    public bool NegativeMarkingEnabled { get; set; } = false;
    public double NegativeMarks { get; set; } = 0.0;
    public bool RandomizeQuestions { get; set; } = false;
    public bool RandomizeOptions { get; set; } = false;
    public bool AllowMultipleAttempts { get; set; } = false;
    public int MaximumAttempts { get; set; } = 1;
    public string ResultReleaseMode { get; set; } = "Immediate";
    public bool ShowCorrectAnswers { get; set; } = true;
    public bool IsRegistrationRequired { get; set; } = true;
    public bool IsPublished { get; set; } = false;
    public string Audience { get; set; } = "School";
    public string? Venue { get; set; }
    public int? Capacity { get; set; }
    public List<int> SelectedQuestionIds { get; set; } = [];
}

public sealed class OnlineExamStudentListViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Standard { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string ExamType { get; set; } = string.Empty;
    public int TotalMarks { get; set; }
    public int QuestionCount { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime? StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public bool IsRegistrationRequired { get; set; }
    public bool IsRegistered { get; set; }
    public bool IsAvailableNow { get; set; }
    public bool IsUpcoming { get; set; }
    public bool IsCompleted { get; set; }
    public int UserAttemptCount { get; set; }
    public int MaxAttempts { get; set; }
    public double? LatestScore { get; set; }
    public double? LatestPercentage { get; set; }
    public string? LatestGrade { get; set; }
    public int? LatestAttemptId { get; set; }
}

public sealed class OnlineExamAttemptViewModel
{
    public int AttemptId { get; set; }
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Standard { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public long RemainingSeconds { get; set; }
    public int TotalQuestions { get; set; }
    public List<StudentAttemptQuestionViewModel> Questions { get; set; } = [];
}

public sealed class StudentAttemptQuestionViewModel
{
    public int QuestionId { get; set; }
    public int Number { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? QuestionImageUrl { get; set; }
    public int Marks { get; set; }
    public List<StudentAttemptOptionViewModel> Options { get; set; } = [];
    public int? SelectedOptionId { get; set; }
    public bool IsMarkedForReview { get; set; }
}

public sealed class StudentAttemptOptionViewModel
{
    public int Id { get; set; }
    public string OptionKey { get; set; } = string.Empty;
    public string OptionText { get; set; } = string.Empty;
    public string? OptionImageUrl { get; set; }
}

public sealed class SaveAnswerRequestModel
{
    public int AttemptId { get; set; }
    public int QuestionId { get; set; }
    public int? SelectedOptionId { get; set; }
    public bool IsMarkedForReview { get; set; }
}

public sealed class OnlineExamResultViewModel
{
    public int AttemptId { get; set; }
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Standard { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public double Score { get; set; }
    public int TotalMarks { get; set; }
    public double Percentage { get; set; }
    public string Grade { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public int CorrectCount { get; set; }
    public int IncorrectCount { get; set; }
    public int UnansweredCount { get; set; }
    public int TimeTakenSeconds { get; set; }
    public string ResultReleaseMode { get; set; } = "Immediate";
    public bool ShowCorrectAnswers { get; set; } = true;
    public bool CanViewReview { get; set; }
    public List<TopicAccuracyViewModel> TopicAnalytics { get; set; } = [];
    public List<QuestionReviewItemViewModel> QuestionReviews { get; set; } = [];
}

public sealed class TopicAccuracyViewModel
{
    public string Topic { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Correct { get; set; }
    public double AccuracyPercentage { get; set; }
}

public sealed class QuestionReviewItemViewModel
{
    public int QuestionNumber { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? QuestionImageUrl { get; set; }
    public int Marks { get; set; }
    public string? Explanation { get; set; }
    public string? StudentSelectedOptionKey { get; set; }
    public string? StudentSelectedOptionText { get; set; }
    public string CorrectOptionKey { get; set; } = string.Empty;
    public string CorrectOptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public bool IsUnanswered { get; set; }
    public List<StudentAttemptOptionViewModel> Options { get; set; } = [];
}

public sealed class OnlineExamAnalyticsViewModel
{
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Standard { get; set; } = string.Empty;
    public int TotalRegistered { get; set; }
    public int TotalAttempts { get; set; }
    public int CompletedAttempts { get; set; }
    public double AverageScore { get; set; }
    public double HighestScore { get; set; }
    public double LowestScore { get; set; }
    public double PassPercentage { get; set; }
    public int AverageTimeTakenSeconds { get; set; }
    public List<QuestionAnalyticsItemViewModel> QuestionAnalytics { get; set; } = [];
    public List<TopicAccuracyViewModel> TopicAnalytics { get; set; } = [];
}

public sealed class QuestionAnalyticsItemViewModel
{
    public int QuestionId { get; set; }
    public int QuestionNumber { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public int TotalAttempts { get; set; }
    public int CorrectCount { get; set; }
    public int IncorrectCount { get; set; }
    public int SkippedCount { get; set; }
    public double CorrectPercentage { get; set; }
}
