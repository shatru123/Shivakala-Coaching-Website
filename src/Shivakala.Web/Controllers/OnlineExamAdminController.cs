using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;
using Shivakala.Core.Services;
using Shivakala.Core.ViewModels;

namespace Shivakala.Web.Controllers;

[Authorize(Roles = "Admin,Teacher"), Route("admin/online-exams")]
public sealed class OnlineExamAdminController(
    IOnlineExamService onlineExamService,
    IQuestionImportService importService,
    IExamRepository examRepo,
    IBatchRepository batchRepo,
    IAuditService audit,
    ILogger<OnlineExamAdminController> logger) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var exams = await examRepo.GetAllAsync(ct);
        var onlineExams = exams.Where(e => e.ExamMode == "Online").ToList();
        return View(onlineExams);
    }

    [HttpGet("wizard")]
    public async Task<IActionResult> Wizard(int? id, CancellationToken ct)
    {
        var vm = await onlineExamService.GetWizardViewModelAsync(id, ct);
        if (vm is null) return NotFound();

        ViewBag.Batches = await batchRepo.GetAllAsync(ct);
        ViewBag.QuestionBank = await onlineExamService.SearchQuestionBankAsync(ct: ct);
        return View(vm);
    }

    [HttpPost("wizard")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveWizard(OnlineExamWizardViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Batches = await batchRepo.GetAllAsync(ct);
            ViewBag.QuestionBank = await onlineExamService.SearchQuestionBankAsync(ct: ct);
            return View("Wizard", model);
        }

        var examId = await onlineExamService.SaveOnlineExamWizardAsync(model, ct);
        TempData["SuccessMessage"] = "Online exam saved successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("question-bank")]
    public async Task<IActionResult> QuestionBank(
        string? search, string? subject, string? standard, string? topic, string? difficulty, CancellationToken ct)
    {
        var questions = await onlineExamService.SearchQuestionBankAsync(search, subject, standard, topic, difficulty, ct);
        ViewBag.Search = search;
        ViewBag.Subject = subject;
        ViewBag.Standard = standard;
        ViewBag.Topic = topic;
        ViewBag.Difficulty = difficulty;
        return View(questions);
    }

    [HttpGet("add-question")]
    public IActionResult CreateQuestion()
    {
        return View(new Question
        {
            QuestionText = "",
            Subject = "General",
            Standard = "General",
            Options = new List<QuestionOption>
            {
                new() { OptionKey = "A", OptionText = "", IsCorrect = true, DisplayOrder = 1 },
                new() { OptionKey = "B", OptionText = "", IsCorrect = false, DisplayOrder = 2 },
                new() { OptionKey = "C", OptionText = "", IsCorrect = false, DisplayOrder = 3 },
                new() { OptionKey = "D", OptionText = "", IsCorrect = false, DisplayOrder = 4 }
            }
        });
    }

    [HttpPost("add-question")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateQuestion(Question model, string correctOptionKey, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(model.QuestionText) || model.Options.Count < 2)
        {
            ModelState.AddModelError("", "Please provide valid question text and at least 2 options.");
            return View(model);
        }

        int order = 1;
        foreach (var opt in model.Options)
        {
            opt.DisplayOrder = order++;
            opt.IsCorrect = string.Equals(opt.OptionKey, correctOptionKey, StringComparison.OrdinalIgnoreCase);
        }

        model.CreatedBy = User.Identity?.Name;
        await onlineExamService.AddQuestionToBankAsync(model, ct);
        TempData["SuccessMessage"] = "Question added to Question Bank.";
        return RedirectToAction(nameof(QuestionBank));
    }

    [HttpGet("import")]
    public IActionResult Import(int? examId)
    {
        ViewBag.ExamId = examId;
        return View();
    }

    [HttpPost("import-preview")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportPreview(
        IFormFile? file, int? examId, string defaultSubject = "General", string defaultStandard = "General", CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
        {
            TempData["WarningMessage"] = "Please select a valid PDF, PPT, PPTX, or Image file to upload.";
            return RedirectToAction(nameof(Import), new { examId });
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExts = new[] { ".pdf", ".ppt", ".pptx", ".jpg", ".jpeg", ".png", ".txt" };
        if (!allowedExts.Contains(ext))
        {
            TempData["WarningMessage"] = "Unsupported file format. Supported formats: PDF, PPT, PPTX, JPG, JPEG, PNG.";
            return RedirectToAction(nameof(Import), new { examId });
        }

        using var stream = file.OpenReadStream();
        var preview = await importService.ParseAndPreviewAsync(stream, file.FileName, defaultSubject, defaultStandard, ct);

        ViewBag.ExamId = examId;
        return View("ImportPreview", preview);
    }

    [HttpPost("confirm-import")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmImport(
        QuestionImportPreviewViewModel model, int? examId, CancellationToken ct)
    {
        var saved = await importService.SaveImportedQuestionsAsync(model, examId, User.Identity?.Name, ct);
        TempData["SuccessMessage"] = $"Successfully imported {saved} questions into the Question Bank.";

        if (examId.HasValue && examId.Value > 0)
        {
            return RedirectToAction(nameof(Wizard), new { id = examId.Value });
        }

        return RedirectToAction(nameof(QuestionBank));
    }

    [HttpPost("{id:int}/publish")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int id, CancellationToken ct)
    {
        await onlineExamService.TogglePublishAsync(id, ct);
        TempData["SuccessMessage"] = "Online exam publication state updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id:int}/analytics")]
    public async Task<IActionResult> Analytics(int id, CancellationToken ct)
    {
        var analytics = await onlineExamService.GetExamAnalyticsAsync(id, ct);
        if (analytics is null) return NotFound();
        return View(analytics);
    }

    [HttpGet("{id:int}/export-csv")]
    public async Task<IActionResult> ExportCsv(int id, CancellationToken ct)
    {
        var bytes = await onlineExamService.ExportResultsToCsvAsync(id, ct);
        return File(bytes, "text/csv", $"OnlineExam_{id}_Results.csv");
    }
}
