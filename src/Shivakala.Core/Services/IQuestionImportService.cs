using Shivakala.Core.ViewModels;

namespace Shivakala.Core.Services;

public interface IQuestionImportService
{
    Task<QuestionImportPreviewViewModel> ParseAndPreviewAsync(
        Stream fileStream,
        string fileName,
        string defaultSubject,
        string defaultStandard,
        CancellationToken ct = default);

    Task<int> SaveImportedQuestionsAsync(
        QuestionImportPreviewViewModel preview,
        int? examId = null,
        string? createdBy = null,
        CancellationToken ct = default);
}
