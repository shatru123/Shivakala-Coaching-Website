using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shivakala.Core.Common;
using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;
using Shivakala.Core.Services;

namespace Shivakala.Web.Controllers;

[Authorize(Roles = "Admin"), Route("admin/fees")]
public sealed class FeeController(
    IFeeRepository feeRepo,
    IStudentRepository studentRepo,
    IAuditService audit) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? month, string? status, CancellationToken ct)
    {
        var m = string.IsNullOrWhiteSpace(month) ? UtcDateTime.CurrentMonthKey() : month;
        ViewBag.Month = m;
        ViewBag.Status = status;
        ViewBag.Payments = await feeRepo.GetAllAsync(m, status, ct);
        ViewBag.TotalCollected = await feeRepo.GetTotalCollectedAsync(m, ct);
        ViewBag.TotalPending   = await feeRepo.GetTotalPendingAsync(ct);
        return View();
    }

    [HttpGet("collect")]
    public async Task<IActionResult> Collect(int? studentId, CancellationToken ct)
    {
        ViewBag.Students = await studentRepo.ListAsync(ct);
        ViewBag.Structures = await feeRepo.GetFeeStructuresAsync(ct);
        ViewBag.SelectedStudentId = studentId;
        if (studentId.HasValue)
            ViewBag.History = await feeRepo.GetByStudentAsync(studentId.Value, ct);
        return View(new FeePayment {
            StudentId = studentId ?? 0,
            PaidDate = UtcDateTime.StartOfToday(),
            Month = UtcDateTime.CurrentMonthKey(),
            FeeType = "Monthly"
        });
    }

    [HttpPost("collect"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Collect(FeePayment model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Students = await studentRepo.ListAsync(ct);
            ViewBag.Structures = await feeRepo.GetFeeStructuresAsync(ct);
            return View(model);
        }
        model.PaidDate = UtcDateTime.EnsureUtc(model.PaidDate);
        model.PaidAmount = model.Amount - model.Discount + model.Fine;
        model.Status = "Paid";
        await feeRepo.AddAsync(model, ct);
        await audit.LogAsync("Created", "FeePayment", model.Id,
            null, $"Receipt:{model.ReceiptNumber}", User.Identity?.Name,
            HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        TempData["SuccessMessage"] = $"Fee collected. Receipt: {model.ReceiptNumber}";
        return RedirectToAction(nameof(Receipt), new { id = model.Id });
    }

    [HttpGet("{id}/receipt")]
    public async Task<IActionResult> Receipt(int id, CancellationToken ct)
    {
        var p = await feeRepo.GetByIdAsync(id, ct);
        if (p is null) return NotFound();
        return View(p);
    }

    [HttpPost("{id}/delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await feeRepo.DeleteAsync(id, ct);
        TempData["SuccessMessage"] = "Payment record deleted.";
        return RedirectToAction(nameof(Index));
    }

    // ── Fee Structure ──────────────────────────────────────────────────────
    [HttpGet("structure")]
    public async Task<IActionResult> Structure(CancellationToken ct)
        => View(await feeRepo.GetFeeStructuresAsync(ct));

    [HttpPost("structure/create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateStructure(FeeStructure model, CancellationToken ct)
    {
        if (ModelState.IsValid) await feeRepo.AddFeeStructureAsync(model, ct);
        TempData["SuccessMessage"] = "Fee structure saved.";
        return RedirectToAction(nameof(Structure));
    }

    [HttpPost("structure/{id}/delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteStructure(int id, CancellationToken ct)
    {
        await feeRepo.DeleteFeeStructureAsync(id, ct);
        TempData["SuccessMessage"] = "Fee structure deleted.";
        return RedirectToAction(nameof(Structure));
    }
}
