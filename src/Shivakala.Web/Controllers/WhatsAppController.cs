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
        ViewBag.ActiveProvider = wa.ActiveProvider;
        ViewBag.IsConfigured = wa.IsConfigured;
        ViewBag.IsAuthenticated = await wa.CheckStatusAsync(ct);
        ViewBag.IsSidecarConfigured = !string.IsNullOrWhiteSpace(whatsAppOptions.Value.BaseUrl);
        ViewBag.SidecarBaseUrl = whatsAppOptions.Value.BaseUrl?.Trim();
        ViewBag.PhoneNumberId = whatsAppOptions.Value.PhoneNumberId?.Trim();

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
        return Json(new
        {
            authenticated = isAuth,
            provider = wa.ActiveProvider,
            configured = wa.IsConfigured
        });
    }

    [HttpGet("links")]
    public async Task<IActionResult> GetLinks(string audience, string message, string? batchId, CancellationToken ct)
    {
        IReadOnlyList<Student> recipients;

        if (audience == "batch" && int.TryParse(batchId, out var bid))
        {
            var batch = await batchRepo.GetByIdWithDetailsAsync(bid, ct);
            recipients = batch?.StudentBatches
                .Where(sb => sb.IsActive && sb.Student != null && !string.IsNullOrWhiteSpace(sb.Student.ParentMobile))
                .Select(sb => sb.Student!)
                .ToList() ?? [];
        }
        else
        {
            var all = await studentRepo.ListAsync(ct);
            recipients = all.Where(s => !string.IsNullOrWhiteSpace(s.ParentMobile)).ToList();
        }

        var links = recipients.Select(s => new
        {
            name = s.FullName,
            mobile = s.ParentMobile,
            url = wa.GenerateWhatsAppWebLink(s.ParentMobile!, message)
        }).ToList();

        return Json(links);
    }

    [HttpPost("disconnect"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Disconnect(CancellationToken ct)
    {
        var disconnected = await wa.DisconnectAsync(ct);
        TempData["SuccessMessage"] = disconnected
            ? "WhatsApp session cleared."
            : "Could not clear WhatsApp session right now.";
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
            ? $"Broadcast sent to {sent}/{mobiles.Count} contacts via {wa.ActiveProvider}."
            : "WhatsApp not configured or authenticated — please check provider settings.";
        return RedirectToAction(nameof(Index));
    }
}
