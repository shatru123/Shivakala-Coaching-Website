using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shivakala.Core.ViewModels;
using Shivakala.Infrastructure.Data;

namespace Shivakala.Web.Controllers;

[Route("teacher")]
public sealed class TeacherPortalController(
    ShivakalaDbContext db,
    ILogger<TeacherPortalController> logger) : Controller
{
    private const string Scheme = CookieAuthenticationDefaults.AuthenticationScheme;

    // ── AUTH ───────────────────────────────────────────────────────────────────
    [HttpGet("login"), AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("Teacher"))
            return RedirectToAction(nameof(Index));
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost("login"), AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl, CancellationToken ct)
    {
        var user = await db.AppUsers.FirstOrDefaultAsync(
            u => u.Username == username && u.Role == "Teacher" && u.IsActive, ct);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            ModelState.AddModelError("", "Invalid username or password.");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // Link to Teacher record
        var teacher = user.TeacherId.HasValue
            ? await db.Teachers.FindAsync([user.TeacherId.Value], ct)
            : null;

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.FullName ?? user.Username),
            new(ClaimTypes.Role, "Teacher"),
            new("UserId",   user.Id.ToString()),
            new("TeacherId", (user.TeacherId ?? 0).ToString())
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme));
        await HttpContext.SignInAsync(Scheme, principal,
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

        user.LastLoginDate = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Teacher logged in: {User}", user.Username);

        return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl) : RedirectToAction(nameof(Index));
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(Scheme);
        return RedirectToAction(nameof(Login));
    }

    // ── DASHBOARD ──────────────────────────────────────────────────────────────
    [HttpGet(""), HttpGet("dashboard"), Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var teacherId = GetTeacherId();
        var teacher   = teacherId > 0 ? await db.Teachers.FindAsync([teacherId], ct) : null;

        ViewBag.Teacher = teacher;
        ViewBag.MyBatches = await db.BatchSubjects
            .Include(bs => bs.Batch)
            .Where(bs => bs.TeacherId == teacherId)
            .Select(bs => bs.Batch)
            .Distinct().ToListAsync(ct);
        ViewBag.TodayHomework = await db.Homeworks
            .Where(h => h.AssignedByTeacherId == teacherId && h.IsActive && h.DueDate >= DateTime.Today)
            .CountAsync(ct);
        ViewBag.TotalHomework = await db.Homeworks
            .Where(h => h.AssignedByTeacherId == teacherId).CountAsync(ct);
        ViewBag.UpcomingExams = await db.Exams
            .Where(e => e.ExamDate >= DateTime.Today && !e.IsPublished).CountAsync(ct);

        return View();
    }

    // ── MARK ATTENDANCE ───────────────────────────────────────────────────────
    [HttpGet("attendance"), Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Attendance(int? batchId, string? date, CancellationToken ct)
    {
        var teacherId = GetTeacherId();
        var myBatches = await db.BatchSubjects
            .Include(bs => bs.Batch)
            .Where(bs => bs.TeacherId == teacherId && bs.Batch!.IsActive)
            .Select(bs => bs.Batch!).Distinct().ToListAsync(ct);

        ViewBag.MyBatches = myBatches;
        ViewBag.SelectedBatchId = batchId;

        if (batchId.HasValue)
        {
            var d = string.IsNullOrWhiteSpace(date)
                ? DateOnly.FromDateTime(DateTime.Today) : DateOnly.Parse(date);
            ViewBag.Date = d;

            var students = await db.StudentBatches
                .Include(sb => sb.Student)
                .Where(sb => sb.BatchId == batchId && sb.IsActive)
                .Select(sb => sb.Student!).ToListAsync(ct);

            var existing = await db.Attendances
                .Where(a => a.BatchId == batchId && a.Date == d.ToString("yyyy-MM-dd"))
                .ToDictionaryAsync(a => a.StudentId, ct);

            ViewBag.Students = students;
            ViewBag.Existing = existing;
        }
        return View();
    }

    [HttpPost("attendance/save"), Authorize(Roles = "Teacher"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveAttendance(int batchId, string date,
        [FromForm] Dictionary<int, string> statuses, CancellationToken ct)
    {
        var teacherId = GetTeacherId();
        foreach (var (studentId, status) in statuses)
        {
            var existing = await db.Attendances.FirstOrDefaultAsync(
                a => a.StudentId == studentId && a.BatchId == batchId && a.Date == date, ct);
            if (existing is null)
                db.Attendances.Add(new Core.Entities.Attendance {
                    StudentId = studentId, BatchId = batchId, Date = DateOnly.Parse(date),
                    Status = status, MarkedByTeacherId = teacherId, CreatedDate = DateTime.UtcNow });
            else
            { existing.Status = status; existing.MarkedByTeacherId = teacherId; }
        }
        await db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = $"Attendance saved for {date}.";
        return RedirectToAction(nameof(Attendance), new { batchId, date });
    }

    // ── MY HOMEWORK ───────────────────────────────────────────────────────────
    [HttpGet("homework"), Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Homework(CancellationToken ct)
    {
        var teacherId = GetTeacherId();
        var list = await db.Homeworks
            .Where(h => h.AssignedByTeacherId == teacherId)
            .OrderByDescending(h => h.CreatedDate).ToListAsync(ct);
        return View(list);
    }

    // ── MY EXAMS / MARKS ──────────────────────────────────────────────────────
    [HttpGet("exams"), Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Exams(CancellationToken ct)
    {
        var list = await db.Exams.Include(e => e.Batch)
            .OrderByDescending(e => e.ExamDate).ToListAsync(ct);
        return View(list);
    }

    // ── HELPER ────────────────────────────────────────────────────────────────
    private int GetTeacherId()
    {
        var claim = User.FindFirst("TeacherId")?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }
}
