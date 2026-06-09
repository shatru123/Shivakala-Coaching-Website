using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shivakala.Core.Common;
using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;

namespace Shivakala.Web.Controllers;

[Authorize(Roles = "Admin"), Route("admin/homework")]
public sealed class HomeworkController(
    IHomeworkRepository hwRepo,
    IBatchRepository batchRepo,
    ITeacherRepository teacherRepo,
    IWebHostEnvironment env) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? standard, string? subject, CancellationToken ct)
    {
        try
        {
            ViewBag.Batches = await batchRepo.GetAllAsync(ct);
            return View(await hwRepo.GetAllAsync(standard, subject, ct));
        }
        catch
        {
            ViewBag.Batches = Array.Empty<Batch>();
            ViewBag.PageLoadWarning = "Homework data is temporarily unavailable. The page is running in safe mode.";
            return View(Array.Empty<Homework>());
        }
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        try
        {
            ViewBag.Batches = await batchRepo.GetAllAsync(ct);
            ViewBag.Teachers = await teacherRepo.GetAllAsync(ct);
        }
        catch
        {
            ViewBag.Batches = Array.Empty<Batch>();
            ViewBag.Teachers = Array.Empty<Teacher>();
            ViewBag.PageLoadWarning = "Teacher or batch data is temporarily unavailable. You can still open the homework form.";
        }
        return View("Form", new Homework
        {
            Title = "", Subject = "", Standard = "",
            AssignedByTeacherId = 0, DueDate = UtcDateTime.StartOfToday().AddDays(2)
        });
    }

    [HttpPost("create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Homework model, IFormFile? attachment, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Batches = await batchRepo.GetAllAsync(ct);
            ViewBag.Teachers = await teacherRepo.GetAllAsync(ct);
            return View("Form", model);
        }
        model.DueDate = UtcDateTime.EnsureUtc(model.DueDate);
        model.AttachmentUrl = await SaveAttachmentAsync(attachment);
        await hwRepo.AddAsync(model, ct);
        TempData["SuccessMessage"] = "Homework assigned.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id}/submissions")]
    public async Task<IActionResult> Submissions(int id, CancellationToken ct)
    {
        var hw = await hwRepo.GetByIdAsync(id, ct);
        if (hw is null) return NotFound();
        ViewBag.Homework = hw;
        return View(await hwRepo.GetSubmissionsAsync(id, ct));
    }

    [HttpPost("{id}/delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await hwRepo.DeleteAsync(id, ct);
        TempData["SuccessMessage"] = "Homework deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<string?> SaveAttachmentAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;
        var dir = Path.Combine(env.WebRootPath, "uploads", "homework");
        Directory.CreateDirectory(dir);
        var name = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        await using var s = System.IO.File.Create(Path.Combine(dir, name));
        await file.CopyToAsync(s);
        return $"/uploads/homework/{name}";
    }
}
