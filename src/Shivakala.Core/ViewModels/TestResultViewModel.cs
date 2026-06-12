using Shivakala.Core.Common;

namespace Shivakala.Core.ViewModels;

public sealed class TestResultViewModel
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Standard { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public int Score { get; set; }
    public int TotalMarks { get; set; }
    public int Rank { get; set; }
    public string? Grade { get; set; }
    public string? Remarks { get; set; }
    public DateTime TestDate { get; set; }
    public string TestTitle { get; set; } = string.Empty;
    public double Percentage => TotalMarks > 0 ? Math.Round(Score * 100.0 / TotalMarks, 1) : 0;
}

public sealed class ResultsPageViewModel
{
    public IReadOnlyList<TestResultViewModel> Results { get; set; } = [];
    public IReadOnlyList<string> AvailableTests { get; set; } = [];
    public string? SelectedTest { get; set; }
    public string? SelectedStandard { get; set; }
    public SeoViewModel Seo { get; set; } = new();
}

public sealed class TestResultFormViewModel
{
    public int Id { get; set; }
    public int? SelectedStudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Standard { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public int Score { get; set; }
    public int TotalMarks { get; set; } = 100;
    public int Rank { get; set; }
    public string? Grade { get; set; }
    public string? Remarks { get; set; }
    public DateTime TestDate { get; set; } = UtcDateTime.StartOfToday();
    public string TestTitle { get; set; } = string.Empty;
    public IReadOnlyList<TestResultStudentOptionViewModel> AvailableStudents { get; set; } = [];
}

public sealed class TestResultStudentOptionViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Standard { get; set; } = string.Empty;
    public string? AdmissionNumber { get; set; }
    public string Mobile { get; set; } = string.Empty;
}
