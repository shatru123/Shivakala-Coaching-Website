using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;
using Shivakala.Core.Services;
using Shivakala.Infrastructure.Configuration;

namespace Shivakala.Web.Controllers;

[Authorize(Roles = "Admin"), Route("admin/whatsapp")]
public sealed class WhatsAppController(
    IWhatsAppService wa,
    INotificationRepository notifRepo,
    IStudentRepository studentRepo,
    IBatchRepository batchRepo,
    IOptions<WhatsAppOptions> whatsAppOptions) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewBag.IsAuthenticated = await wa.CheckStatusAsync(ct);
        ViewBag.IsSidecarConfigured = !string.IsNullOrWhiteSpace(whatsAppOptions.Value.BaseUrl);
        ViewBag.SidecarBaseUrl = whatsAppOptions.Value.BaseUrl?.Trim();
        try
        {
            ViewBag.Batches = await batchRepo.GetAllAsync(ct);
            ViewBag.RecentNotifs = await notifRepo.GetAllAsync(20, ct);
        }
        catch
        {
            ViewBag.Batches = Array.Empty<Batch>();
            ViewBag.RecentNotifs = Array.Empty<Notification>();
            ViewBag.PageLoadWarning = "Batch or broadcast history data is temporarily unavailable. WhatsApp connection controls are still shown.";
        }
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
    public async Task<IActionResult> Status(CancellationToken ct)
    {
        var isAuth = await wa.CheckStatusAsync(ct);
        return Json(new { authenticated = isAuth });
    }

    [HttpPost("disconnect"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Disconnect(CancellationToken ct)
    {
        var disconnected = await wa.DisconnectAsync(ct);
        TempData["SuccessMessage"] = disconnected
            ? "WhatsApp disconnected. You can now scan the QR with a different account."
            : "Could not disconnect WhatsApp right now. Please verify the sidecar is running and reachable.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("broadcast"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Broadcast(
        string audience, string message, string? batchId, CancellationToken ct)
    {
        var isAuthenticated = await wa.CheckStatusAsync(ct);
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
        if (isAuthenticated)
            sent = await wa.BroadcastAsync(mobiles, message, ct);

        var notif = await notifRepo.AddAsync(new Notification
        {
            Title = "WhatsApp Broadcast",
            Message = message,
            Channel = "WhatsApp",
            Audience = audience,
            Status = isAuthenticated ? "Sent" : "Failed",
            DeliveredCount = sent,
            FailedCount = mobiles.Count - sent,
            SentAt = DateTime.UtcNow
        }, ct);

        TempData["SuccessMessage"] = isAuthenticated
            ? $"Broadcast sent to {sent}/{mobiles.Count} contacts."
            : "WhatsApp not authenticated — please scan the QR first.";
        return RedirectToAction(nameof(Index));
    }
}
