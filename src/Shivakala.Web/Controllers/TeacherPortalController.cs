using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Common;
using Shivakala.Core.Interfaces;
using Shivakala.Core.Services;
using Shivakala.Core.ViewModels;
using Shivakala.Infrastructure.Data;

namespace Shivakala.Web.Controllers;

[Route("teacher")]
public sealed class TeacherPortalController(
    ShivakalaDbContext db,
    ITeacherRepository teacherRepo,
    IPortalUserService portalUsers,
    ILogger<TeacherPortalController> logger) : Controller
{
    private const string Scheme = CookieAuthenticationDefaults.AuthenticationScheme;

    // ── AUTH ──────────────────────────────────────────────────────────────────
    [HttpGet("login"), AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("Teacher"))
            return Redirect("/teacher");
        ViewBag.ReturnUrl = returnUrl;
        return View("Login");                       // explicit view name
    }

    [HttpPost("login"), AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Authenticate(
        string username, string password, string? returnUrl, CancellationToken ct)
    {
        var user = await portalUsers.ValidateCredentialsAsync(username, password, "Teacher", ct);
        if (user is null)
        {
            ModelState.AddModelError("", "Invalid username or password.");
            ViewBag.ReturnUrl = returnUrl;
            return View("Login");                   // explicit — not View()
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name,    user.FullName ?? user.Username),
            new(ClaimTypes.Role,    "Teacher"),
            new("UserId",           user.Id.ToString()),
            new("TeacherId",        (user.TeacherId ?? 0).ToString())
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme));
        await HttpContext.SignInAsync(Scheme, principal,
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

        user.LastLoginDate = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Teacher logged in: {User}", user.Username);

        return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl) : Redirect("/teacher");
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(Scheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet("change-password"), Authorize(Roles = "Teacher")]
    public IActionResult ChangePassword() => View("ChangePassword", new ChangePasswordViewModel());

    [HttpPost("change-password"), Authorize(Roles = "Teacher"), ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View("ChangePassword", vm);

        if (!int.TryParse(User.FindFirst("UserId")?.Value, out var userId) || userId <= 0)
            return Forbid();

        var result = await portalUsers.ChangePasswordAsync(userId, vm.CurrentPassword, vm.NewPassword, ct);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage);
            return View("ChangePassword", vm);
        }

        TempData["SuccessMessage"] = "Your password has been updated.";
        return RedirectToAction(nameof(ChangePassword));
    }

    // ── DASHBOARD ─────────────────────────────────────────────────────────────
    [HttpGet(""), HttpGet("dashboard"), Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var tid = GetTeacherId();
        var todayUtc = UtcDateTime.StartOfToday();
        ViewBag.Teacher = tid > 0 ? await teacherRepo.GetByIdAsync(tid, ct) : null;

        ViewBag.MyBatches = await db.BatchSubjects
            .Include(bs => bs.Batch)
            .Where(bs => bs.TeacherId == tid && bs.Batch!.IsActive)
            .Select(bs => bs.Batch!).Distinct()
            .OrderBy(b => b.Standard).ThenBy(b => b.Name)
            .ToListAsync(ct);

        ViewBag.TodayHomework = await db.Homeworks
            .Where(h => h.AssignedByTeacherId == tid && h.IsActive && h.DueDate >= todayUtc)
            .CountAsync(ct);
        ViewBag.TotalHomework = await db.Homeworks
            .CountAsync(h => h.AssignedByTeacherId == tid, ct);
        ViewBag.UpcomingExams = await db.Exams
            .CountAsync(e => e.ExamDate >= todayUtc && !e.IsPublished, ct);
        ViewBag.TotalStudents = await db.StudentBatches
            .Where(sb => sb.IsActive && db.BatchSubjects
                .Any(bs => bs.BatchId == sb.BatchId && bs.TeacherId == tid))
            .Select(sb => sb.StudentId).Distinct().CountAsync(ct);

        return View("Index");
    }

    // ── ATTENDANCE ────────────────────────────────────────────────────────────
    [HttpGet("attendance"), Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Attendance(int? batchId, string? date, CancellationToken ct)
    {
        var tid = GetTeacherId();

        var myBatches = await db.BatchSubjects
            .Include(bs => bs.Batch)
            .Where(bs => bs.TeacherId == tid && bs.Batch!.IsActive)
            .Select(bs => bs.Batch!).Distinct()
            .OrderBy(b => b.Standard).ThenBy(b => b.Name)
            .ToListAsync(ct);

        ViewBag.MyBatches       = myBatches;
        ViewBag.SelectedBatchId = batchId;

        if (batchId.HasValue)
        {
            // ── Fix: DateOnly comparison — do NOT use Equals(DateOnly, string) ──
            var d = string.IsNullOrWhiteSpace(date)
                ? UtcDateTime.Today()
                : DateOnly.Parse(date);
            ViewBag.Date = d;

            var students = await db.StudentBatches
                .Include(sb => sb.Student)
                .Where(sb => sb.BatchId == batchId && sb.IsActive)
                .Select(sb => sb.Student!)
                .OrderBy(s => s.FullName)
                .ToListAsync(ct);

            // Direct DateOnly == DateOnly comparison — EF Core translates correctly
            var existing = await db.Attendances
                .Where(a => a.BatchId == batchId && a.Date == d)
                .ToDictionaryAsync(a => a.StudentId, ct);

            ViewBag.Students = students;
            ViewBag.Existing = existing;
        }
        return View("Attendance");
    }

    [HttpPost("attendance/save"), Authorize(Roles = "Teacher"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveAttendance(
        int batchId, string date,
        [FromForm] Dictionary<int, string> statuses, CancellationToken ct)
    {
        var tid = GetTeacherId();
        var d   = DateOnly.Parse(date);

        foreach (var (studentId, status) in statuses)
        {
            var existing = await db.Attendances
                .FirstOrDefaultAsync(a => a.StudentId == studentId
                                       && a.BatchId   == batchId
                                       && a.Date      == d, ct);
            if (existing is null)
                db.Attendances.Add(new Core.Entities.Attendance {
                    StudentId         = studentId,
                    BatchId           = batchId,
                    Date              = d,
                    Status            = status,
                    MarkedByTeacherId = tid,
                    CreatedDate       = DateTime.UtcNow
                });
            else
            {
                existing.Status            = status;
                existing.MarkedByTeacherId = tid;
            }
        }
        await db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = $"Attendance saved for {d:dd MMM yyyy}.";
        return RedirectToAction(nameof(Attendance), new { batchId, date });
    }

    // ── HOMEWORK ──────────────────────────────────────────────────────────────
    [HttpGet("homework"), Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Homework(CancellationToken ct)
    {
        var tid  = GetTeacherId();
        var list = await db.Homeworks
            .Include(h => h.Batch)
            .Where(h => h.AssignedByTeacherId == tid)
            .OrderByDescending(h => h.CreatedDate)
            .ToListAsync(ct);

        ViewBag.Batches = await db.BatchSubjects
            .Include(bs => bs.Batch)
            .Where(bs => bs.TeacherId == tid && bs.Batch!.IsActive)
            .Select(bs => bs.Batch!).Distinct().ToListAsync(ct);

        return View("Homework", list);
    }

    [HttpPost("homework/create"), Authorize(Roles = "Teacher"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateHomework(
        string title, string subject, string standard,
        int? batchId, DateTime dueDate, string? description,
        IFormFile? attachment, CancellationToken ct)
    {
        var tid = GetTeacherId();
        if (tid == 0) return Forbid();
        dueDate = UtcDateTime.EnsureUtc(dueDate);

        string? attachmentUrl = null;
        if (attachment is { Length: > 0 })
        {
            var dir  = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "homework");
            Directory.CreateDirectory(dir);
            var name = $"{Guid.NewGuid()}{Path.GetExtension(attachment.FileName)}";
            await using var s = System.IO.File.Create(Path.Combine(dir, name));
            await attachment.CopyToAsync(s, ct);
            attachmentUrl = $"/uploads/homework/{name}";
        }

        db.Homeworks.Add(new Core.Entities.Homework {
            Title               = title,
            Subject             = subject,
            Standard            = standard,
            BatchId             = batchId,
            AssignedByTeacherId = tid,
            DueDate             = dueDate,
            Description         = description,
            AttachmentUrl       = attachmentUrl,
            IsActive            = true,
            CreatedDate         = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = $"Homework '{title}' assigned.";
        return RedirectToAction(nameof(Homework));
    }

    [HttpPost("homework/{id}/delete"), Authorize(Roles = "Teacher"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteHomework(int id, CancellationToken ct)
    {
        var tid = GetTeacherId();
        var hw  = await db.Homeworks.FirstOrDefaultAsync(
            h => h.Id == id && h.AssignedByTeacherId == tid, ct);
        if (hw is not null) { db.Homeworks.Remove(hw); await db.SaveChangesAsync(ct); }
        TempData["SuccessMessage"] = "Homework deleted.";
        return RedirectToAction(nameof(Homework));
    }

    // ── EXAMS & MARKS ─────────────────────────────────────────────────────────
    [HttpGet("exams"), Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Exams(CancellationToken ct)
    {
        var tid = GetTeacherId();

        // Show exams for batches this teacher teaches
        var myBatchIds = await db.BatchSubjects
            .Where(bs => bs.TeacherId == tid)
            .Select(bs => bs.BatchId).Distinct().ToListAsync(ct);

        var list = await db.Exams
            .Include(e => e.Batch)
            .Where(e => e.BatchId == null || myBatchIds.Contains(e.BatchId!.Value))
            .OrderByDescending(e => e.ExamDate).ToListAsync(ct);

        return View("Exams", list);
    }

    [HttpGet("exams/{id}/marks"), Authorize(Roles = "Teacher")]
    public async Task<IActionResult> ExamMarks(int id, CancellationToken ct)
    {
        var exam = await db.Exams
            .Include(e => e.Batch)
            .Include(e => e.Results).ThenInclude(r => r.Student)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (exam is null) return NotFound();

        // If no results yet, build them from students in the batch
        if (!exam.Results.Any() && exam.BatchId.HasValue)
        {
            var students = await db.StudentBatches
                .Include(sb => sb.Student)
                .Where(sb => sb.BatchId == exam.BatchId && sb.IsActive)
                .Select(sb => sb.Student!).ToListAsync(ct);

            foreach (var s in students)
                exam.Results.Add(new Core.Entities.ExamResult
                    { ExamId = id, StudentId = s.Id, Student = s });
        }

        ViewBag.Exam = exam;
        return View("ExamMarks", exam.Results.OrderBy(r => r.Student?.FullName).ToList());
    }

    [HttpPost("exams/{id}/marks"), Authorize(Roles = "Teacher"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveMarks(
        int id,
        [FromForm] Dictionary<int, int?> marks,
        [FromForm] Dictionary<int, bool> absent,
        CancellationToken ct)
    {
        var exam = await db.Exams.FindAsync([id], ct);
        if (exam is null) return NotFound();

        foreach (var (studentId, mark) in marks)
        {
            var isAbsent = absent.GetValueOrDefault(studentId);
            var existing = await db.ExamResults.FirstOrDefaultAsync(
                r => r.ExamId == id && r.StudentId == studentId, ct);

            if (existing is null)
                db.ExamResults.Add(new Core.Entities.ExamResult {
                    ExamId        = id,
                    StudentId     = studentId,
                    MarksObtained = isAbsent ? null : mark,
                    IsAbsent      = isAbsent,
                    CreatedDate   = DateTime.UtcNow
                });
            else
            {
                existing.MarksObtained = isAbsent ? null : mark;
                existing.IsAbsent      = isAbsent;
            }
        }
        await db.SaveChangesAsync(ct);

        // Recalculate ranks
        var results = await db.ExamResults
            .Where(r => r.ExamId == id && !r.IsAbsent && r.MarksObtained.HasValue)
            .OrderByDescending(r => r.MarksObtained).ToListAsync(ct);
        int rank = 1;
        foreach (var r in results)
        {
            r.Rank  = rank++;
            var pct = (double)r.MarksObtained!.Value / exam.TotalMarks * 100;
            r.Grade = pct switch {
                >= 90 => "A+", >= 80 => "A", >= 70 => "B+",
                >= 60 => "B",  >= 50 => "C", _      => "D"
            };
        }
        await db.SaveChangesAsync(ct);

        TempData["SuccessMessage"] = "Marks saved and ranks recalculated.";
        return RedirectToAction(nameof(ExamMarks), new { id });
    }

    // ── MY STUDENTS ───────────────────────────────────────────────────────────
    [HttpGet("students"), Authorize(Roles = "Teacher")]
    public async Task<IActionResult> MyStudents(int? batchId, CancellationToken ct)
    {
        var tid = GetTeacherId();

        var myBatches = await db.BatchSubjects
            .Include(bs => bs.Batch)
            .Where(bs => bs.TeacherId == tid && bs.Batch!.IsActive)
            .Select(bs => bs.Batch!).Distinct().ToListAsync(ct);

        ViewBag.MyBatches       = myBatches;
        ViewBag.SelectedBatchId = batchId;

        List<Core.Entities.Student> students = [];
        if (batchId.HasValue)
        {
            students = await db.StudentBatches
                .Include(sb => sb.Student)
                .Where(sb => sb.BatchId == batchId && sb.IsActive)
                .Select(sb => sb.Student!).OrderBy(s => s.FullName)
                .ToListAsync(ct);
        }
        return View("MyStudents", students);
    }

    // ── HELPER ────────────────────────────────────────────────────────────────
    private int GetTeacherId()
    {
        var v = User.FindFirst("TeacherId")?.Value;
        return int.TryParse(v, out var id) ? id : 0;
    }
}
