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

[Route("parent")]
public sealed class ParentPortalController(
    ShivakalaDbContext db,
    IPortalUserService portalUsers,
    ILogger<ParentPortalController> logger) : Controller
{
    private const string Scheme = CookieAuthenticationDefaults.AuthenticationScheme;

    [HttpGet("login"), AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("Parent"))
            return RedirectToAction(nameof(Index));
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost("login"), AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Authenticate(string username, string password, string? returnUrl, CancellationToken ct)
    {
        var user = await portalUsers.ValidateCredentialsAsync(username, password, "Parent", ct);
        if (user is null)
        {
            ModelState.AddModelError("", "Invalid mobile number or password.");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        var mobile = user.Username;
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.FullName ?? user.Username),
            new(ClaimTypes.Role, "Parent"),
            new("UserId", user.Id.ToString()),
            new("ParentMobile", mobile),
            new("StudentId", (user.StudentId ?? 0).ToString())
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme));
        await HttpContext.SignInAsync(Scheme, principal,
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

        user.LastLoginDate = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Parent logged in: {User}", user.Username);

        return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl) : Redirect("/parent");
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(Scheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet("change-password"), Authorize(Roles = "Parent")]
    public IActionResult ChangePassword() => View("ChangePassword", new ChangePasswordViewModel());

    [HttpPost("change-password"), Authorize(Roles = "Parent"), ValidateAntiForgeryToken]
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

    [HttpGet(""), HttpGet("dashboard"), Authorize(Roles = "Parent")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var mobile = User.FindFirst("ParentMobile")?.Value ?? "";
        var children = await db.Students
            .Where(s => s.Status == "Admitted" &&
                (s.ParentMobile == mobile || s.Mobile == mobile))
            .OrderBy(s => s.FullName)
            .ToListAsync(ct);

        if (children.Count == 0 && int.TryParse(User.FindFirst("StudentId")?.Value, out var sid) && sid > 0)
        {
            var one = await db.Students.FindAsync([sid], ct);
            if (one is not null) children = [one];
        }

        var studentIds = children.Select(c => c.Id).ToList();

        ViewBag.Children = children;
        ViewBag.Attendance = studentIds.Count == 0 ? [] : await db.Attendances
            .Where(a => studentIds.Contains(a.StudentId))
            .OrderByDescending(a => a.Date)
            .Take(30)
            .ToListAsync(ct);

        ViewBag.FeePayments = studentIds.Count == 0 ? [] : await db.FeePayments
            .Where(f => studentIds.Contains(f.StudentId))
            .OrderByDescending(f => f.PaidDate)
            .Take(12)
            .ToListAsync(ct);

        ViewBag.ExamResults = studentIds.Count == 0 ? [] : await db.ExamResults
            .Include(e => e.Exam)
            .Where(e => studentIds.Contains(e.StudentId))
            .OrderByDescending(e => e.CreatedDate)
            .Take(20)
            .ToListAsync(ct);

        return View();
    }

    [HttpGet("attendance"), Authorize(Roles = "Parent")]
    public async Task<IActionResult> Attendance(CancellationToken ct)
    {
        var mobile = User.FindFirst("ParentMobile")?.Value ?? "";
        var studentIds = await db.Students
            .Where(s => s.Status == "Admitted" && (s.ParentMobile == mobile || s.Mobile == mobile))
            .Select(s => s.Id)
            .ToListAsync(ct);

        var list = await db.Attendances
            .Where(a => studentIds.Contains(a.StudentId))
            .OrderByDescending(a => a.Date)
            .ToListAsync(ct);

        ViewBag.Children = await db.Students.Where(s => studentIds.Contains(s.Id)).ToListAsync(ct);
        return View(list);
    }

    [HttpGet("fees"), Authorize(Roles = "Parent")]
    public async Task<IActionResult> Fees(CancellationToken ct)
    {
        var mobile = User.FindFirst("ParentMobile")?.Value ?? "";
        var studentIds = await db.Students
            .Where(s => s.Status == "Admitted" && (s.ParentMobile == mobile || s.Mobile == mobile))
            .Select(s => s.Id)
            .ToListAsync(ct);

        var list = await db.FeePayments
            .Where(f => studentIds.Contains(f.StudentId))
            .OrderByDescending(f => f.PaidDate)
            .ToListAsync(ct);

        ViewBag.Children = await db.Students.Where(s => studentIds.Contains(s.Id)).ToListAsync(ct);
        return View(list);
    }
}
