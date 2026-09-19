using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shivakala.Core.Services;
using Shivakala.Core.ViewModels;

namespace Shivakala.Web.Controllers;

[Route("admin/mpsc")]
[Authorize(Roles = "Admin,SuperAdmin")]
public sealed class MpscAdminController(
    IMpscStudentService mpscService,
    ILogger<MpscAdminController> logger) : Controller
{
    // ── Admin Dashboard ──────────────────────────────────────────────────────

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var vm = await mpscService.GetAdminDashboardAsync(ct);
        return View(vm);
    }

    // ── Candidates Directory ─────────────────────────────────────────────────

    [HttpGet("candidates")]
    public async Task<IActionResult> Candidates([FromQuery] MpscCandidateListFilterViewModel filter, CancellationToken ct)
    {
        var result = await mpscService.GetCandidatesAsync(filter, ct);
        return View(result);
    }

    [HttpGet("candidates/{id:int}")]
    public async Task<IActionResult> CandidateDetail(int id, CancellationToken ct)
    {
        var detail = await mpscService.GetCandidateDetailAsync(id, ct);
        if (detail is null) return NotFound();

        return View(detail);
    }

    [HttpPost("candidates/{id:int}/toggle-status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id, CancellationToken ct)
    {
        var (success, message) = await mpscService.ToggleCandidateStatusAsync(id, ct);
        if (success) TempData["SuccessMessage"] = message;
        else TempData["ErrorMessage"] = message;

        return RedirectToAction(nameof(CandidateDetail), new { id });
    }

    [HttpPost("candidates/{id:int}/reset-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(int id, string newPassword, CancellationToken ct)
    {
        var (success, message) = await mpscService.ResetCandidatePasswordAsync(id, newPassword, ct);
        if (success) TempData["SuccessMessage"] = message;
        else TempData["ErrorMessage"] = message;

        return RedirectToAction(nameof(CandidateDetail), new { id });
    }

    [HttpGet("candidates/export")]
    public async Task<IActionResult> ExportCsv([FromQuery] MpscCandidateListFilterViewModel filter, CancellationToken ct)
    {
        var bytes = await mpscService.ExportCandidatesCsvAsync(filter, ct);
        var filename = $"MPSC_Candidates_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv";
        return File(bytes, "text/csv", filename);
    }

    // ── Bulk CSV Import ──────────────────────────────────────────────────────

    [HttpGet("import")]
    public IActionResult Import()
    {
        return View();
    }

    [HttpPost("import-preview")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportPreview(IFormFile? csvFile, CancellationToken ct)
    {
        if (csvFile == null || csvFile.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select a valid CSV file to upload.";
            return RedirectToAction(nameof(Import));
        }

        using var stream = csvFile.OpenReadStream();
        var preview = await mpscService.PreviewImportCsvAsync(stream, ct);
        preview.FileName = csvFile.FileName;

        return View("ImportPreview", preview);
    }

    [HttpPost("import-commit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportCommit(List<MpscImportRow> rows, CancellationToken ct)
    {
        if (rows == null || rows.Count == 0)
        {
            TempData["WarningMessage"] = "No candidate records found to import.";
            return RedirectToAction(nameof(Import));
        }

        var (imported, skipped, errors) = await mpscService.CommitImportAsync(rows, ct);

        TempData["SuccessMessage"] = $"Import completed: {imported} candidates imported successfully, {skipped} skipped.";
        if (errors.Count > 0)
        {
            TempData["ErrorMessage"] = string.Join("; ", errors.Take(5));
        }

        return RedirectToAction(nameof(Candidates));
    }

    // ── Offline Exam & Check-In Management ───────────────────────────────────

    [HttpGet("offline-exams")]
    public async Task<IActionResult> OfflineExams(CancellationToken ct)
    {
        var exams = await mpscService.GetMpscExamsForAdminAsync(ct);
        return View(exams);
    }

    [HttpGet("offline-exams/{id:int}/check-in")]
    public async Task<IActionResult> CheckIn(int id, string? search, CancellationToken ct)
    {
        var vm = await mpscService.GetOfflineCheckInListAsync(id, search, ct);
        if (vm is null) return NotFound();

        return View(vm);
    }

    [HttpPost("offline-exams/{id:int}/update-attendance")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAttendance(
        int id, int registrationId, string status, string? seatNumber, string? remarks, CancellationToken ct)
    {
        var (success, message) = await mpscService.UpdateOfflineAttendanceAsync(registrationId, status, seatNumber, remarks, ct);
        if (success) TempData["SuccessMessage"] = message;
        else TempData["ErrorMessage"] = message;

        return RedirectToAction(nameof(CheckIn), new { id });
    }

    [HttpGet("offline-exams/{id:int}/enter-marks")]
    public async Task<IActionResult> EnterMarks(int id, CancellationToken ct)
    {
        var vm = await mpscService.GetOfflineResultEntryModelAsync(id, ct);
        if (vm is null) return NotFound();

        return View(vm);
    }

    [HttpPost("offline-exams/{id:int}/enter-marks")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnterMarks(OfflineResultEntryViewModel model, CancellationToken ct)
    {
        var (success, message) = await mpscService.SaveOfflineResultsAsync(model, ct);
        if (success)
            TempData["SuccessMessage"] = message;
        else
            TempData["ErrorMessage"] = message;

        return RedirectToAction(nameof(EnterMarks), new { id = model.ExamId });
    }
}
