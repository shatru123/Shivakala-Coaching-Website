using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shivakala.Core.Common;
using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;
using Shivakala.Core.Services;

namespace Shivakala.Web.Controllers;

[Authorize(Roles = "Admin"), Route("admin/exams")]
public sealed class ExamController(
    IExamRepository examRepo,
    IBatchRepository batchRepo,
    IAuditService audit) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await examRepo.GetAllAsync(ct));

    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        try
        {
            ViewBag.Batches = await batchRepo.GetAllAsync(ct);
        }
        catch
        {
            ViewBag.Batches = Array.Empty<Batch>();
            ViewBag.PageLoadWarning = "Batch data is temporarily unavailable. You can still prepare the exam form.";
        }
        return View("Form", new Exam { Title = "", Standard = "", Subject = "", ExamDate = UtcDateTime.StartOfToday() });
    }

    [HttpPost("create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Exam model, CancellationToken ct)
    {
        if (!ModelState.IsValid) { ViewBag.Batches = await batchRepo.GetAllAsync(ct); return View("Form", model); }
        model.ExamDate = UtcDateTime.EnsureUtc(model.ExamDate);
        await examRepo.AddAsync(model, ct);
        TempData["SuccessMessage"] = "Exam scheduled.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id}/marks")]
    public async Task<IActionResult> Marks(int id, CancellationToken ct)
    {
        try
        {
            var exam = await examRepo.GetByIdWithResultsAsync(id, ct);
            if (exam is null) return NotFound();
            ViewBag.Exam = exam;
            return View(exam.Results);
        }
        catch
        {
            TempData["WarningMessage"] = "Exam marks could not be loaded right now.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("{id}/marks"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveMarks(int id,
        [FromForm] Dictionary<int, int?> marks,
        [FromForm] Dictionary<int, bool> absent,
        CancellationToken ct)
    {
        var exam = await examRepo.GetByIdWithResultsAsync(id, ct);
        if (exam is null) return NotFound();

        var results = marks.Select(kvp => new ExamResult
        {
            ExamId = id,
            StudentId = kvp.Key,
            MarksObtained = absent.GetValueOrDefault(kvp.Key) ? null : kvp.Value,
            IsAbsent = absent.GetValueOrDefault(kvp.Key)
        });
        await examRepo.BulkUpsertResultsAsync(results, ct);
        await examRepo.RecalculateRanksAsync(id, ct);
        TempData["SuccessMessage"] = "Marks saved and ranks recalculated.";
        return RedirectToAction(nameof(Marks), new { id });
    }

    [HttpPost("{id}/publish"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int id, CancellationToken ct)
    {
        var exam = await examRepo.GetByIdWithResultsAsync(id, ct);
        if (exam is null) return NotFound();
        exam.IsPublished = true;
        await examRepo.UpdateAsync(exam, ct);
        TempData["SuccessMessage"] = "Results published.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id}/delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await examRepo.DeleteAsync(id, ct);
        TempData["SuccessMessage"] = "Exam deleted.";
        return RedirectToAction(nameof(Index));
    }
}
