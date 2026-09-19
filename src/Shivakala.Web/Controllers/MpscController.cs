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

[Route("mpsc")]
public sealed class MpscController(
    IMpscStudentService mpscService,
    IPortalUserService portalUsers,
    IOnlineExamService onlineExamService,
    ShivakalaDbContext db,
    ILogger<MpscController> logger) : Controller
{
    private const string Scheme = CookieAuthenticationDefaults.AuthenticationScheme;

    // ── Public Overview Landing Page ─────────────────────────────────────────

    [HttpGet("")]
    [AllowAnonymous]
    public IActionResult Index()
    {
        return View();
    }

    // ── Registration ─────────────────────────────────────────────────────────

    [HttpGet("register")]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("MpscStudent"))
            return RedirectToAction(nameof(Dashboard));

        var model = new MpscRegistrationViewModel
        {
            State = "Maharashtra",
            TargetAttemptYear = DateTime.UtcNow.Year,
            HighestQualification = "Graduate",
            PreferredExam = "Rajyaseva",
            PreparationLevel = "Beginner",
            Category = "Open"
        };
        return View(model);
    }

    [HttpPost("register")]
    [AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(MpscRegistrationViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(model);

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var (success, message, candidate, generatedPassword) = await mpscService.RegisterCandidateAsync(model, ip, ct);

        if (!success || candidate is null)
        {
            ModelState.AddModelError(string.Empty, message);
            return View(model);
        }

        TempData["RegNumber"] = candidate.RegistrationNumber;
        TempData["FullName"] = candidate.FullName;
        TempData["Email"] = candidate.Email;
        TempData["Mobile"] = candidate.Mobile;
        TempData["TargetExam"] = candidate.PreferredExam;

        return RedirectToAction(nameof(Success), new { regNumber = candidate.RegistrationNumber });
    }

    [HttpGet("success")]
    [AllowAnonymous]
    public async Task<IActionResult> Success(string regNumber, CancellationToken ct)
    {
        var candidate = await db.MpscStudents.FirstOrDefaultAsync(s => s.RegistrationNumber == regNumber, ct);
        if (candidate is null && TempData["RegNumber"] == null)
            return RedirectToAction(nameof(Index));

        ViewBag.RegNumber = regNumber ?? TempData["RegNumber"]?.ToString();
        ViewBag.FullName = candidate?.FullName ?? TempData["FullName"]?.ToString();
        ViewBag.Email = candidate?.Email ?? TempData["Email"]?.ToString();
        ViewBag.Mobile = candidate?.Mobile ?? TempData["Mobile"]?.ToString();
        ViewBag.TargetExam = candidate?.PreferredExam ?? TempData["TargetExam"]?.ToString();

        return View();
    }

    // ── Candidate Authentication ─────────────────────────────────────────────

    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("MpscStudent"))
            return RedirectToAction(nameof(Dashboard));

        ViewBag.ReturnUrl = returnUrl;
        return View(new MpscLoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost("login")]
    [AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(MpscLoginViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await portalUsers.ValidateCredentialsAsync(model.Identifier, model.Password, "MpscStudent", ct);
        if (user is null || !user.MpscStudentId.HasValue)
        {
            ModelState.AddModelError(string.Empty, "Invalid Registration Number / Email / Mobile or password.");
            return View(model);
        }

        var candidate = await mpscService.GetCandidateByIdAsync(user.MpscStudentId.Value, ct);
        if (candidate is null || !candidate.IsActive)
        {
            ModelState.AddModelError(string.Empty, "This candidate account is currently inactive. Please contact administration.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, candidate.FullName),
            new(ClaimTypes.Role, "MpscStudent"),
            new("UserId", user.Id.ToString()),
            new("MpscStudentId", candidate.Id.ToString()),
            new("RegistrationNumber", candidate.RegistrationNumber),
            new("Mobile", candidate.Mobile),
            new("Email", candidate.Email)
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme));
        var authProps = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(12)
        };

        await HttpContext.SignInAsync(Scheme, principal, authProps);

        user.LastLoginDate = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("MPSC Candidate logged in: {RegNo} (Id #{Id})", candidate.RegistrationNumber, candidate.Id);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return LocalRedirect(model.ReturnUrl);

        return RedirectToAction(nameof(Dashboard));
    }

    [HttpGet("logout")]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(Scheme);
        return RedirectToAction(nameof(Index));
    }

    // ── Candidate Portal Area ────────────────────────────────────────────────

    [HttpGet("dashboard")]
    [Authorize(Roles = "MpscStudent")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
    {
        var candidateId = GetCurrentMpscStudentId();
        if (candidateId == 0) return RedirectToAction(nameof(Login));

        var vm = await mpscService.GetDashboardAsync(candidateId, ct);
        return View(vm);
    }

    [HttpGet("profile")]
    [Authorize(Roles = "MpscStudent")]
    public async Task<IActionResult> Profile(CancellationToken ct)
    {
        var candidateId = GetCurrentMpscStudentId();
        if (candidateId == 0) return RedirectToAction(nameof(Login));

        var profile = await mpscService.GetProfileAsync(candidateId, ct);
        if (profile is null) return NotFound();

        return View(profile);
    }

    [HttpPost("profile")]
    [Authorize(Roles = "MpscStudent"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(MpscProfileViewModel model, CancellationToken ct)
    {
        var candidateId = GetCurrentMpscStudentId();
        if (candidateId == 0 || model.Id != candidateId) return Forbid();

        if (!ModelState.IsValid)
            return View(model);

        var (success, message) = await mpscService.UpdateProfileAsync(model, ct);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, message);
            return View(model);
        }

        TempData["SuccessMessage"] = "Your profile has been updated successfully.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet("change-password")]
    [Authorize(Roles = "MpscStudent")]
    public IActionResult ChangePassword()
    {
        return View(new MpscChangePasswordViewModel());
    }

    [HttpPost("change-password")]
    [Authorize(Roles = "MpscStudent"), ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(MpscChangePasswordViewModel model, CancellationToken ct)
    {
        var candidateId = GetCurrentMpscStudentId();
        if (candidateId == 0) return RedirectToAction(nameof(Login));

        if (!ModelState.IsValid)
            return View(model);

        var (success, message) = await mpscService.ChangePasswordAsync(candidateId, model.CurrentPassword, model.NewPassword, ct);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, message);
            return View(model);
        }

        TempData["SuccessMessage"] = "Your password has been changed successfully.";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpGet("exams")]
    [Authorize(Roles = "MpscStudent")]
    public async Task<IActionResult> Exams(CancellationToken ct)
    {
        var candidateId = GetCurrentMpscStudentId();
        if (candidateId == 0) return RedirectToAction(nameof(Login));

        var exams = await mpscService.GetAvailableExamsAsync(candidateId, ct);
        return View(exams);
    }

    [HttpPost("exams/register/{examId:int}")]
    [Authorize(Roles = "MpscStudent"), ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterExam(int examId, CancellationToken ct)
    {
        var candidateId = GetCurrentMpscStudentId();
        if (candidateId == 0) return RedirectToAction(nameof(Login));

        var (success, message) = await mpscService.RegisterForExamAsync(examId, candidateId, ct);
        if (success)
            TempData["SuccessMessage"] = message;
        else
            TempData["WarningMessage"] = message;

        return RedirectToAction(nameof(Exams));
    }

    [HttpGet("results")]
    [Authorize(Roles = "MpscStudent")]
    public async Task<IActionResult> Results(CancellationToken ct)
    {
        var candidateId = GetCurrentMpscStudentId();
        if (candidateId == 0) return RedirectToAction(nameof(Login));

        var results = await mpscService.GetCandidateResultsAsync(candidateId, ct);
        return View(results);
    }

    // ── Helper ───────────────────────────────────────────────────────────────

    private int GetCurrentMpscStudentId()
    {
        var claim = User.FindFirstValue("MpscStudentId");
        return int.TryParse(claim, out var id) ? id : 0;
    }
}
