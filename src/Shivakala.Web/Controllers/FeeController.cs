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
    IAuditService audit,
    IWhatsAppService whatsAppService) : Controller
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
    public async Task<IActionResult> Collect(FeePayment model, bool sendReceiptOnWhatsApp, CancellationToken ct)
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
        model = await feeRepo.GetByIdAsync(model.Id, ct) ?? model;

        var successMessage = $"Fee collected. Receipt: {model.ReceiptNumber}";
        if (sendReceiptOnWhatsApp)
        {
            var receiptSent = await TrySendReceiptOnWhatsAppAsync(model, ct);
            if (receiptSent)
            {
                successMessage += " Receipt sent on WhatsApp.";
            }
            else
            {
                TempData["WarningMessage"] = "Fee was collected, but the receipt could not be sent on WhatsApp. You can still print the receipt.";
            }
        }

        TempData["SuccessMessage"] = successMessage;
        return RedirectToAction(nameof(Receipt), new { id = model.Id });
    }

    [HttpGet("{id}/receipt")]
    public async Task<IActionResult> Receipt(int id, CancellationToken ct)
    {
        var p = await feeRepo.GetByIdAsync(id, ct);
        if (p is null) return NotFound();
        return View(p);
    }

    [HttpPost("{id}/send-whatsapp"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SendReceiptOnWhatsApp(int id, CancellationToken ct)
    {
        var payment = await feeRepo.GetByIdAsync(id, ct);
        if (payment is null) return NotFound();

        if (await TrySendReceiptOnWhatsAppAsync(payment, ct))
        {
            TempData["SuccessMessage"] = $"Receipt {payment.ReceiptNumber} sent on WhatsApp.";
        }
        else
        {
            TempData["WarningMessage"] = "Receipt could not be sent on WhatsApp right now. Please verify the parent's mobile number and WhatsApp connection.";
        }

        return RedirectToAction(nameof(Receipt), new { id });
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

    private async Task<bool> TrySendReceiptOnWhatsAppAsync(FeePayment payment, CancellationToken ct)
    {
        var mobile = payment.Student?.ParentMobile;
        if (string.IsNullOrWhiteSpace(mobile))
            mobile = payment.Student?.Mobile;

        var normalizedMobile = NormalizeIndianMobile(mobile);
        if (string.IsNullOrWhiteSpace(normalizedMobile))
            return false;

        var message = BuildReceiptWhatsAppMessage(payment);
        return await whatsAppService.SendMessageAsync(normalizedMobile, message, ct);
    }

    private static string BuildReceiptWhatsAppMessage(FeePayment payment)
    {
        var paidDate = payment.PaidDate == default
            ? UtcDateTime.NowInAppTimeZone().ToString("dd MMM yyyy")
            : payment.PaidDate.ToLocalTime().ToString("dd MMM yyyy");

        return string.Join('\n', new[]
        {
            "Shivakala Coaching Classes",
            "Fee Receipt Confirmation",
            $"Receipt No: {payment.ReceiptNumber}",
            $"Student: {payment.Student?.FullName ?? "Student"}",
            $"Standard: {payment.Student?.Standard ?? "-"}",
            $"Fee Type: {payment.FeeType}",
            $"Month: {payment.Month}",
            $"Amount Paid: ₹{payment.PaidAmount:N2}",
            $"Payment Mode: {payment.PaymentMode}",
            $"Date: {paidDate}",
            string.IsNullOrWhiteSpace(payment.TransactionRef) ? string.Empty : $"Transaction Ref: {payment.TransactionRef}",
            "Thank you."
        }.Where(static line => !string.IsNullOrWhiteSpace(line)));
    }

    private static string? NormalizeIndianMobile(string? mobile)
    {
        if (string.IsNullOrWhiteSpace(mobile))
            return null;

        var digits = new string(mobile.Where(char.IsDigit).ToArray());
        if (digits.Length == 10)
            return $"91{digits}";

        if (digits.Length == 12 && digits.StartsWith("91", StringComparison.Ordinal))
            return digits;

        return digits.Length >= 10 ? digits : null;
    }
}
