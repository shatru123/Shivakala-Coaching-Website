using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;
using Shivakala.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Shivakala.Web.Controllers;

[Authorize(Roles = "Admin"), Route("admin/timetable")]
public sealed class TimetableController(
    IBatchRepository batchRepo,
    ITeacherRepository teacherRepo,
    ShivakalaDbContext db) : Controller
{
    private static readonly string[] Days = ["Monday","Tuesday","Wednesday","Thursday","Friday","Saturday","Sunday"];

    [HttpGet("")]
    public async Task<IActionResult> Index(int? batchId, CancellationToken ct)
    {
        ViewBag.Days = Days;
        try
        {
            ViewBag.Batches = await batchRepo.GetAllAsync(ct);
            if (batchId.HasValue)
            {
                var supportsTeacherAboutFields = await TeacherSchemaCompatibility.SupportsAboutPageFieldsAsync(db, ct);
                var slots = supportsTeacherAboutFields
                    ? await db.TimetableSlots
                        .Include(s => s.Teacher)
                        .Where(s => s.BatchId == batchId && s.IsActive)
                        .OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime)
                        .ToListAsync(ct)
                    : await db.TimetableSlots
                        .Where(s => s.BatchId == batchId && s.IsActive)
                        .OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime)
                        .ToListAsync(ct);
                if (!supportsTeacherAboutFields)
                {
                    var teacherNames = await TeacherSchemaCompatibility.GetTeacherNamesFallbackAsync(db, ct);
                    foreach (var slot in slots.Where(s => s.TeacherId.HasValue))
                    {
                        if (slot.TeacherId.HasValue && teacherNames.TryGetValue(slot.TeacherId.Value, out var teacherName))
                            slot.Teacher = new Teacher { Id = slot.TeacherId.Value, FullName = teacherName, Mobile = string.Empty };
                    }
                }
                ViewBag.Slots = slots;
                ViewBag.SelectedBatchId = batchId;
            }
        }
        catch
        {
            ViewBag.Batches = Array.Empty<Batch>();
            ViewBag.Slots = new List<TimetableSlot>();
            ViewBag.PageLoadWarning = "Timetable data is temporarily unavailable. The page is running in safe mode.";
        }
        return View();
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(int batchId, CancellationToken ct)
    {
        try
        {
            ViewBag.Batch = await batchRepo.GetByIdWithDetailsAsync(batchId, ct);
            ViewBag.Teachers = await teacherRepo.GetAllAsync(ct);
        }
        catch
        {
            ViewBag.Batch = null;
            ViewBag.Teachers = Array.Empty<Teacher>();
            ViewBag.PageLoadWarning = "Batch or teacher data is temporarily unavailable. You can still open the timetable form.";
        }
        ViewBag.Days = Days;
        return View("Form", new TimetableSlot { BatchId = batchId, Subject = "" });
    }

    [HttpPost("create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TimetableSlot model, CancellationToken ct)
    {
        // Conflict detection: same teacher, same day, overlapping time
        if (model.TeacherId.HasValue)
        {
            var conflict = await db.TimetableSlots.AnyAsync(s =>
                s.TeacherId == model.TeacherId &&
                s.DayOfWeek == model.DayOfWeek &&
                s.IsActive &&
                s.StartTime < model.EndTime && s.EndTime > model.StartTime, ct);
            if (conflict)
                ModelState.AddModelError("", "Teacher already has a slot overlapping this time.");
        }
        if (!ModelState.IsValid)
        {
            ViewBag.Batch = await batchRepo.GetByIdWithDetailsAsync(model.BatchId, ct);
            ViewBag.Teachers = await teacherRepo.GetAllAsync(ct);
            ViewBag.Days = Days;
            return View("Form", model);
        }
        db.TimetableSlots.Add(model);
        await db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "Slot added.";
        return RedirectToAction(nameof(Index), new { batchId = model.BatchId });
    }

    [HttpPost("{id}/delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int batchId, CancellationToken ct)
    {
        var slot = await db.TimetableSlots.FindAsync([id], ct);
        if (slot is not null) { db.TimetableSlots.Remove(slot); await db.SaveChangesAsync(ct); }
        return RedirectToAction(nameof(Index), new { batchId });
    }
}
