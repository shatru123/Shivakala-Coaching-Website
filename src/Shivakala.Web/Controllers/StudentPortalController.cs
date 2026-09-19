using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Services;
using Shivakala.Core.ViewModels;
using Shivakala.Infrastructure.Data;

namespace Shivakala.Web.Controllers;

[Route("student")]
public sealed class StudentPortalController(
    ShivakalaDbContext db,
    IPortalUserService portalUsers,
    IOnlineExamService onlineExamService,
    ILogger<StudentPortalController> logger) : Controller
{
    private const string Scheme = CookieAuthenticationDefaults.AuthenticationScheme;

    [HttpGet("login"), AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("Student"))
            return RedirectToAction(nameof(Index));

        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost("login"), AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Authenticate(string username, string password, string? returnUrl, CancellationToken ct)
    {
        var user = await portalUsers.ValidateCredentialsAsync(username, password, "Student", ct);
        if (user is null)
        {
            ModelState.AddModelError("", "Invalid student username/mobile/roll number or password.");
            ViewBag.ReturnUrl = returnUrl;
            return View("Login");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.FullName ?? user.Username),
            new(ClaimTypes.Role, "Student"),
            new("UserId", user.Id.ToString()),
            new("StudentId", (user.StudentId ?? 0).ToString())
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme));
        await HttpContext.SignInAsync(Scheme, principal,
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

        user.LastLoginDate = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Student logged in: {User} (StudentId #{Id})", user.Username, user.StudentId);

        return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl) : Redirect("/online-exams");
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(Scheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet(""), Authorize(Roles = "Student")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var studentIdClaim = User.FindFirstValue("StudentId");
        if (!int.TryParse(studentIdClaim, out var studentId) || studentId == 0)
        {
            return RedirectToAction(nameof(Login));
        }

        var student = await db.Students
            .Include(s => s.StudentBatches)
                .ThenInclude(sb => sb.Batch)
            .FirstOrDefaultAsync(s => s.Id == studentId, ct);

        if (student is null) return NotFound();

        ViewBag.Student = student;
        ViewBag.Exams = await onlineExamService.GetStudentExamsAsync(studentId, ct);
        return View();
    }

    [HttpGet("my-results"), Authorize(Roles = "Student")]
    public async Task<IActionResult> MyResults(CancellationToken ct)
    {
        var studentIdClaim = User.FindFirstValue("StudentId");
        if (!int.TryParse(studentIdClaim, out var studentId) || studentId == 0)
        {
            return RedirectToAction(nameof(Login));
        }

        var attempts = await db.OnlineExamAttempts
            .Include(a => a.Exam)
            .Where(a => a.StudentId == studentId && (a.Status == "Submitted" || a.Status == "AutoSubmitted"))
            .OrderByDescending(a => a.SubmittedAt)
            .ToListAsync(ct);

        return View(attempts);
    }
}
