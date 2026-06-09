using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;
using Shivakala.Core.Services;

namespace Shivakala.Web.Controllers;

[Authorize(Roles = "Admin"), Route("admin/batches")]
public sealed class BatchController(
    IBatchRepository batchRepo,
    ITeacherRepository teacherRepo,
    IStudentRepository studentRepo,
    IAuditService audit,
    ILogger<BatchController> logger) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        try
        {
            ViewBag.Batches = await batchRepo.GetAllAsync(ct);
        }
        catch
        {
            ViewBag.Batches = Array.Empty<Batch>();
            ViewBag.PageLoadWarning = "Batch data is temporarily unavailable. The page is running in safe mode.";
        }
        return View();
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        try
        {
            ViewBag.Teachers = await teacherRepo.GetAllAsync(ct);
        }
        catch
        {
            ViewBag.Teachers = Array.Empty<Teacher>();
            ViewBag.PageLoadWarning = "Teacher data is temporarily unavailable. You can still open the batch form.";
        }
        return View("Form", new Batch { Name = "", Standard = "", AcademicYear = $"{DateTime.UtcNow.Year}-{DateTime.UtcNow.Year+1}" });
    }

    [HttpPost("create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Batch model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Teachers = await teacherRepo.GetAllAsync(ct);
            return View("Form", model);
        }
        await batchRepo.AddAsync(model, ct);
        await audit.LogAsync("Created", "Batch", model.Id, null,
            $"{{Name:{model.Name}}}", User.Identity?.Name,
            HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        TempData["SuccessMessage"] = "Batch created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Detail(int id, CancellationToken ct)
    {
        try
        {
            var batch = await batchRepo.GetByIdWithDetailsAsync(id, ct);
            if (batch is null) return NotFound();
            ViewBag.AllStudents = await studentRepo.ListAsync(ct);
            ViewBag.AllTeachers = await teacherRepo.GetAllAsync(ct);
            return View(batch);
        }
        catch
        {
            TempData["WarningMessage"] = "Batch details could not be loaded right now.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("{id}/assign-student"), ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignStudent(int id, int studentId, CancellationToken ct)
    {
        var batch = await batchRepo.GetByIdWithDetailsAsync(id, ct);
        if (batch is null) return NotFound();
        if (batch.StudentBatches.All(sb => sb.StudentId != studentId))
        {
            batch.StudentBatches.Add(new StudentBatch { StudentId = studentId, BatchId = id });
            await batchRepo.UpdateAsync(batch, ct);
        }
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost("{id}/remove-student"), ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveStudent(int id, int studentBatchId, CancellationToken ct)
    {
        var batch = await batchRepo.GetByIdWithDetailsAsync(id, ct);
        if (batch is null) return NotFound();
        var sb = batch.StudentBatches.FirstOrDefault(x => x.Id == studentBatchId);
        if (sb is not null) { batch.StudentBatches.Remove(sb); await batchRepo.UpdateAsync(batch, ct); }
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost("{id}/delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await batchRepo.DeleteAsync(id, ct);
        TempData["SuccessMessage"] = "Batch deleted.";
        return RedirectToAction(nameof(Index));
    }
}
