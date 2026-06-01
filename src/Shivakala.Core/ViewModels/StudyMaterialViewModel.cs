using Microsoft.AspNetCore.Http;

namespace Shivakala.Core.ViewModels;

public sealed class StudyMaterialViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string TitleMarathi { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string Standard { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string MaterialType { get; set; } = "QuestionPaper";
    public long FileSizeBytes { get; set; }
    public bool IsActive { get; set; }
    public DateTime UploadedDate { get; set; }
    public string FileSizeDisplay => FileSizeBytes switch {
        < 1024 => $"{FileSizeBytes} B",
        < 1024 * 1024 => $"{FileSizeBytes / 1024.0:F1} KB",
        _ => $"{FileSizeBytes / (1024.0 * 1024):F1} MB"
    };
}

public sealed class StudyMaterialsPageViewModel
{
    public IReadOnlyList<StudyMaterialViewModel> Materials { get; set; } = [];
    public string? SelectedStandard { get; set; }
    public string? SelectedSubject { get; set; }
    public string? SelectedType { get; set; }
    public SeoViewModel Seo { get; set; } = new();
}

public sealed class StudyMaterialFormViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string TitleMarathi { get; set; } = string.Empty;
    public string Standard { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string MaterialType { get; set; } = "QuestionPaper";
    public bool IsActive { get; set; } = true;
    public IFormFile? File { get; set; }
}
