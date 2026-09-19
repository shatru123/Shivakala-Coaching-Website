using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shivakala.Core.Services;
using Shivakala.Core.ViewModels;

namespace Shivakala.Web.Controllers;

[Route("online-exams")]
public sealed class OnlineExamController(
    IOnlineExamService onlineExamService,
    IMpscStudentService mpscService,
    ILogger<OnlineExamController> logger) : Controller
{
    [HttpGet("")]
    [AllowAnonymous]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("MpscStudent"))
        {
            return RedirectToAction("Exams", "Mpsc");
        }

        var isStudentLoggedIn = User.Identity?.IsAuthenticated == true && User.IsInRole("Student");
        var studentIdClaim = User.FindFirstValue("StudentId");

        if (isStudentLoggedIn && int.TryParse(studentIdClaim, out var studentId) && studentId > 0)
        {
            var studentExams = await onlineExamService.GetStudentExamsAsync(studentId, ct);
            ViewBag.IsStudentLoggedIn = true;
            return View("StudentIndex", studentExams);
        }

        var publicExams = await onlineExamService.GetStudentExamsAsync(0, ct);
        ViewBag.IsStudentLoggedIn = false;
        return View("PublicIndex", publicExams);
    }

    [HttpGet("{id:int}/details")]
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var wizard = await onlineExamService.GetWizardViewModelAsync(id, ct);
        if (wizard is null || !wizard.IsPublished) return NotFound();

        var isStudentLoggedIn = User.Identity?.IsAuthenticated == true && User.IsInRole("Student");
        var isMpscLoggedIn = User.Identity?.IsAuthenticated == true && User.IsInRole("MpscStudent");
        ViewBag.IsStudentLoggedIn = isStudentLoggedIn || isMpscLoggedIn;
        ViewBag.IsMpsc = isMpscLoggedIn;

        if (isStudentLoggedIn && int.TryParse(User.FindFirstValue("StudentId"), out var studentId) && studentId > 0)
        {
            var studentExams = await onlineExamService.GetStudentExamsAsync(studentId, ct);
            ViewBag.StudentExamInfo = studentExams.FirstOrDefault(e => e.Id == id);
        }
        else if (isMpscLoggedIn && int.TryParse(User.FindFirstValue("MpscStudentId"), out var mpscId) && mpscId > 0)
        {
            var mpscExams = await mpscService.GetAvailableExamsAsync(mpscId, ct);
            var mpscExam = mpscExams.FirstOrDefault(e => e.ExamId == id);
            if (mpscExam != null)
            {
                ViewBag.StudentExamInfo = new OnlineExamStudentListViewModel
                {
                    Id = mpscExam.ExamId,
                    Title = mpscExam.Title,
                    Subject = mpscExam.Subject,
                    IsRegistered = mpscExam.IsRegistered,
                    IsAvailableNow = mpscExam.CanAttempt,
                    IsCompleted = mpscExam.HasAttempted,
                    TotalMarks = mpscExam.TotalMarks,
                    DurationMinutes = mpscExam.DurationMinutes
                };
            }
        }

        return View(wizard);
    }

    [HttpPost("{id:int}/register")]
    [Authorize(Roles = "Student,MpscStudent"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(int id, CancellationToken ct)
    {
        if (User.IsInRole("MpscStudent"))
        {
            if (!int.TryParse(User.FindFirstValue("MpscStudentId"), out var mpscStudentId) || mpscStudentId == 0)
                return RedirectToAction("Login", "Mpsc");

            var (success, message) = await mpscService.RegisterForExamAsync(id, mpscStudentId, ct);
            if (success) TempData["SuccessMessage"] = message;
            else TempData["WarningMessage"] = message;

            return RedirectToAction(nameof(Details), new { id });
        }

        if (!int.TryParse(User.FindFirstValue("StudentId"), out var studentId) || studentId == 0)
            return RedirectToAction("Login", "StudentPortal");

        var regSuccess = await onlineExamService.RegisterStudentForExamAsync(id, studentId, ct);
        if (regSuccess)
        {
            TempData["SuccessMessage"] = "You have successfully registered for the online exam!";
        }
        else
        {
            TempData["WarningMessage"] = "Unable to register for this exam. Please check schedule and eligibility.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:int}/start")]
    [Authorize(Roles = "Student,MpscStudent"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(int id, CancellationToken ct)
    {
        if (User.IsInRole("MpscStudent"))
        {
            if (!int.TryParse(User.FindFirstValue("MpscStudentId"), out var mpscStudentId) || mpscStudentId == 0)
                return RedirectToAction("Login", "Mpsc");

            var mpscResult = await onlineExamService.StartOrResumeMpscAttemptAsync(id, mpscStudentId, ct);
            if (!mpscResult.Success || mpscResult.Attempt is null)
            {
                TempData["WarningMessage"] = mpscResult.Message;
                return RedirectToAction(nameof(Details), new { id });
            }

            return RedirectToAction(nameof(Attempt), new { id, attemptId = mpscResult.Attempt.AttemptId });
        }

        if (!int.TryParse(User.FindFirstValue("StudentId"), out var studentId) || studentId == 0)
            return RedirectToAction("Login", "StudentPortal");

        var result = await onlineExamService.StartOrResumeAttemptAsync(id, studentId, ct);
        if (!result.Success || result.Attempt is null)
        {
            TempData["WarningMessage"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        return RedirectToAction(nameof(Attempt), new { id, attemptId = result.Attempt.AttemptId });
    }

    [HttpGet("{id:int}/attempt/{attemptId:int}")]
    [Authorize(Roles = "Student,MpscStudent")]
    public async Task<IActionResult> Attempt(int id, int attemptId, CancellationToken ct)
    {
        if (User.IsInRole("MpscStudent"))
        {
            if (!int.TryParse(User.FindFirstValue("MpscStudentId"), out var mpscStudentId) || mpscStudentId == 0)
                return RedirectToAction("Login", "Mpsc");

            var mpscResult = await onlineExamService.StartOrResumeMpscAttemptAsync(id, mpscStudentId, ct);
            if (!mpscResult.Success || mpscResult.Attempt is null || mpscResult.Attempt.AttemptId != attemptId)
            {
                TempData["WarningMessage"] = mpscResult.Message;
                return RedirectToAction("Exams", "Mpsc");
            }

            return View(mpscResult.Attempt);
        }

        if (!int.TryParse(User.FindFirstValue("StudentId"), out var studentId) || studentId == 0)
            return RedirectToAction("Login", "StudentPortal");

        var result = await onlineExamService.StartOrResumeAttemptAsync(id, studentId, ct);
        if (!result.Success || result.Attempt is null || result.Attempt.AttemptId != attemptId)
        {
            TempData["WarningMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        return View(result.Attempt);
    }

    [HttpPost("save-answer")]
    [Authorize(Roles = "Student,MpscStudent")]
    public async Task<IActionResult> SaveAnswer([FromBody] SaveAnswerRequestModel model, CancellationToken ct)
    {
        if (User.IsInRole("MpscStudent"))
        {
            if (!int.TryParse(User.FindFirstValue("MpscStudentId"), out var mpscStudentId) || mpscStudentId == 0)
                return Unauthorized(new { success = false, message = "Login required" });

            var mpscRes = await onlineExamService.AutoSaveMpscAnswerAsync(model, mpscStudentId, ct);
            return Json(new { success = mpscRes.Success, message = mpscRes.Message });
        }

        if (!int.TryParse(User.FindFirstValue("StudentId"), out var studentId) || studentId == 0)
            return Unauthorized(new { success = false, message = "Login required" });

        var result = await onlineExamService.AutoSaveAnswerAsync(model, studentId, ct);
        return Json(new { success = result.Success, message = result.Message });
    }

    [HttpPost("submit")]
    [Authorize(Roles = "Student,MpscStudent"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int attemptId, bool isAutoSubmit = false, CancellationToken ct = default)
    {
        if (User.IsInRole("MpscStudent"))
        {
            if (!int.TryParse(User.FindFirstValue("MpscStudentId"), out var mpscStudentId) || mpscStudentId == 0)
                return RedirectToAction("Login", "Mpsc");

            var mpscRes = await onlineExamService.SubmitMpscAttemptAsync(attemptId, mpscStudentId, isAutoSubmit, ct);
            if (!mpscRes.Success || !mpscRes.AttemptId.HasValue)
            {
                TempData["WarningMessage"] = mpscRes.Message;
                return RedirectToAction("Exams", "Mpsc");
            }

            TempData["SuccessMessage"] = isAutoSubmit
                ? "Time expired! Your examination was automatically submitted."
                : "Examination submitted successfully!";

            var mpscAttemptResult = await onlineExamService.GetMpscAttemptResultAsync(mpscRes.AttemptId.Value, mpscStudentId, ct);
            if (mpscAttemptResult is null) return RedirectToAction("Exams", "Mpsc");

            return RedirectToAction(nameof(Result), new { id = mpscAttemptResult.ExamId, attemptId = mpscRes.AttemptId.Value });
        }

        if (!int.TryParse(User.FindFirstValue("StudentId"), out var studentId) || studentId == 0)
            return RedirectToAction("Login", "StudentPortal");

        var result = await onlineExamService.SubmitAttemptAsync(attemptId, studentId, isAutoSubmit, ct);
        if (!result.Success || !result.AttemptId.HasValue)
        {
            TempData["WarningMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = isAutoSubmit
            ? "Time expired! Your examination was automatically submitted."
            : "Examination submitted successfully!";

        var attemptResult = await onlineExamService.GetAttemptResultAsync(result.AttemptId.Value, studentId, isStudent: true, ct);
        if (attemptResult is null) return RedirectToAction(nameof(Index));

        return RedirectToAction(nameof(Result), new { id = attemptResult.ExamId, attemptId = result.AttemptId.Value });
    }

    [HttpGet("{id:int}/result/{attemptId:int}")]
    [Authorize]
    public async Task<IActionResult> Result(int id, int attemptId, CancellationToken ct)
    {
        var isStudent = User.IsInRole("Student");
        var isMpsc = User.IsInRole("MpscStudent");
        int currentUserId = 0;

        if (isMpsc)
        {
            int.TryParse(User.FindFirstValue("MpscStudentId"), out currentUserId);
        }
        else if (isStudent)
        {
            int.TryParse(User.FindFirstValue("StudentId"), out currentUserId);
        }
        else
        {
            int.TryParse(User.FindFirstValue("UserId"), out currentUserId);
        }

        var result = await onlineExamService.GetAttemptResultAsync(attemptId, currentUserId, isStudent || isMpsc, ct);
        if (result is null)
        {
            TempData["WarningMessage"] = "Exam result is not accessible or does not exist.";
            return isMpsc ? RedirectToAction("Exams", "Mpsc") : RedirectToAction(nameof(Index));
        }

        return View(result);
    }
}
