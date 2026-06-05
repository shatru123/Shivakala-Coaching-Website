using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shivakala.Core.Common;
using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;

namespace Shivakala.Web.Controllers;

[Authorize(Roles = "Admin"), Route("admin/attendance")]
public sealed class AttendanceController(
    IAttendanceRepository attendanceRepo,
    IBatchRepository batchRepo,
    IStudentRepository studentRepo) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewBag.Batches = await batchRepo.GetAllAsync(ct);
        return View();
    }

    // GET: mark?batchId=1&date=2026-06-02
    [HttpGet("mark")]
    public async Task<IActionResult> Mark(int batchId, string? date, CancellationToken ct)
    {
        var d = string.IsNullOrWhiteSpace(date) ? UtcDateTime.Today() : DateOnly.Parse(date);
        var batch = await batchRepo.GetByIdWithDetailsAsync(batchId, ct);
        if (batch is null) return NotFound();
        var existing = await attendanceRepo.GetByBatchAndDateAsync(batchId, d, ct);
        ViewBag.Batch = batch; ViewBag.Date = d;
        ViewBag.Existing = existing.ToDictionary(a => a.StudentId);
        return View(batch.StudentBatches.Where(sb => sb.IsActive).Select(sb => sb.Student).ToList());
    }

    // POST: mark
    [HttpPost("mark"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Mark(int batchId, string date,
        [FromForm] Dictionary<int, string> statuses, CancellationToken ct)
    {
        var d = DateOnly.Parse(date);
        var records = statuses.Select(kvp => new Attendance
        {
            StudentId = kvp.Key, BatchId = batchId, Date = d, Status = kvp.Value,
            MarkedByTeacherId = null, CreatedDate = DateTime.UtcNow
        });
        await attendanceRepo.BulkUpsertAsync(records, ct);
        TempData["SuccessMessage"] = $"Attendance saved for {d:dd MMM yyyy}.";
        return RedirectToAction(nameof(Mark), new { batchId, date });
    }

    // GET: report?studentId=1
    [HttpGet("report")]
    public async Task<IActionResult> Report(int? studentId, int? batchId,
        string? from, string? to, CancellationToken ct)
    {
        var today = UtcDateTime.Today();
        var f = string.IsNullOrWhiteSpace(from) ? today.AddDays(-30) : DateOnly.Parse(from);
        var t = string.IsNullOrWhiteSpace(to)   ? today : DateOnly.Parse(to);
        ViewBag.Batches  = await batchRepo.GetAllAsync(ct);
        ViewBag.Students = await studentRepo.ListAsync(ct);
        ViewBag.From = f; ViewBag.To = t;

        if (studentId.HasValue)
        {
            var records = await attendanceRepo.GetByStudentAsync(studentId.Value, f, t, ct);
            var pct = await attendanceRepo.GetAttendancePercentageAsync(studentId.Value, f, t, ct);
            ViewBag.StudentId = studentId; ViewBag.Records = records; ViewBag.Percentage = pct;
        }
        else if (batchId.HasValue)
        {
            var summary = await attendanceRepo.GetBatchAttendanceSummaryAsync(batchId.Value, f, t, ct);
            ViewBag.BatchId = batchId; ViewBag.Summary = summary;
        }
        return View();
    }
}
