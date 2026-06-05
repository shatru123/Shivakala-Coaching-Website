using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;
using Shivakala.Core.Services;

namespace Shivakala.Web.Controllers;

[Authorize(Roles = "Admin"), Route("admin/whatsapp")]
public sealed class WhatsAppController(
    IWhatsAppService wa,
    INotificationRepository notifRepo,
    IStudentRepository studentRepo,
    IBatchRepository batchRepo) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewBag.IsAuthenticated = wa.IsAuthenticated;
        ViewBag.Batches = await batchRepo.GetAllAsync(ct);
        ViewBag.RecentNotifs = await notifRepo.GetAllAsync(20, ct);
        return View();
    }

    [HttpGet("qr.png")]
    public async Task<IActionResult> Qr(CancellationToken ct)
    {
        var qr = await wa.GetQrCodeAsync(ct);
        if (qr is null) return NoContent();
        return File(qr, "image/png");
    }

    [HttpGet("status")]
    public IActionResult Status() => Json(new { authenticated = wa.IsAuthenticated });

    [HttpPost("broadcast"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Broadcast(
        string audience, string message, string? batchId, CancellationToken ct)
    {
        IReadOnlyList<string> mobiles;

        if (audience == "batch" && int.TryParse(batchId, out var bid))
        {
            var batch = await batchRepo.GetByIdWithDetailsAsync(bid, ct);
            mobiles = batch?.StudentBatches
                .Where(sb => sb.IsActive && !string.IsNullOrWhiteSpace(sb.Student?.ParentMobile))
                .Select(sb => sb.Student!.ParentMobile!).ToList() ?? [];
        }
        else
        {
            var all = await studentRepo.ListAsync(ct);
            mobiles = all.Where(s => !string.IsNullOrWhiteSpace(s.ParentMobile))
                         .Select(s => s.ParentMobile!).Distinct().ToList();
        }

        int sent = 0;
        if (wa.IsAuthenticated)
            sent = await wa.BroadcastAsync(mobiles, message, ct);

        var notif = await notifRepo.AddAsync(new Notification
        {
            Title = "WhatsApp Broadcast",
            Message = message,
            Channel = "WhatsApp",
            Audience = audience,
            Status = wa.IsAuthenticated ? "Sent" : "Failed",
            DeliveredCount = sent,
            FailedCount = mobiles.Count - sent,
            SentAt = DateTime.UtcNow
        }, ct);

        TempData["SuccessMessage"] = wa.IsAuthenticated
            ? $"Broadcast sent to {sent}/{mobiles.Count} contacts."
            : "WhatsApp not authenticated — please scan the QR first.";
        return RedirectToAction(nameof(Index));
    }
}
