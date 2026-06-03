using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;
using Shivakala.Core.Services;
using Shivakala.Infrastructure.Data;

namespace Shivakala.Web.Controllers;

[Authorize, Route("admin/teachers")]
public sealed class TeacherController(
    ITeacherRepository repo,
    IPortalUserService portalUsers,
    ShivakalaDbContext db,
    IAuditService audit,
    IWebHostEnvironment env,
    ILogger<TeacherController> logger) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewBag.PortalUsernames = await db.AppUsers
            .Where(u => u.Role == "Teacher" && u.TeacherId != null)
            .ToDictionaryAsync(u => u.TeacherId!.Value, u => u.Username, ct);
        return View(await repo.GetAllAsync(ct));
    }

    [HttpGet("create")]
    public IActionResult Create() => View("Form", new Teacher { FullName = "", Mobile = "" });

    [HttpPost("create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Teacher model, IFormFile? photo, string? portalUsername, string? portalPassword, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View("Form", model);
        model.PhotoUrl = await SavePhotoAsync(photo);
        await repo.AddAsync(model, ct);
        var portalUser = await portalUsers.EnsureTeacherAccountAsync(model.Id, portalUsername, portalPassword, ct);
        await audit.LogAsync("Created", "Teacher", model.Id, null,
            $"{{Name:{model.FullName}}}", User.Identity?.Name, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        TempData["SuccessMessage"] = $"Teacher added. Portal login — username: {portalUser.Username}, password: {(string.IsNullOrWhiteSpace(portalPassword) ? "last 4 digits of mobile" : "(as set)")}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id}/edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var t = await repo.GetByIdAsync(id, ct);
        if (t is null) return NotFound();
        return View("Form", t);
    }

    [HttpPost("{id}/edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Teacher model, IFormFile? photo, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View("Form", model);
        var existing = await repo.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();

        if (photo is { Length: > 0 })
        {
            DeletePhoto(existing.PhotoUrl);
            existing.PhotoUrl = await SavePhotoAsync(photo);
        }
        existing.FullName = model.FullName; existing.Mobile = model.Mobile;
        existing.Email = model.Email; existing.Qualification = model.Qualification;
        existing.Specialisation = model.Specialisation; existing.Address = model.Address;
        existing.EmployeeCode = model.EmployeeCode; existing.MonthlySalary = model.MonthlySalary;
        existing.JoiningDate = model.JoiningDate; existing.IsActive = model.IsActive;
        existing.AdminNotes = model.AdminNotes;

        await repo.UpdateAsync(existing, ct);
        await audit.LogAsync("Updated", "Teacher", id, null, null, User.Identity?.Name,
            HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        TempData["SuccessMessage"] = "Teacher updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id}/delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var t = await repo.GetByIdAsync(id, ct);
        if (t is not null) { DeletePhoto(t.PhotoUrl); await repo.DeleteAsync(id, ct); }
        TempData["SuccessMessage"] = "Teacher deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<string?> SavePhotoAsync(IFormFile? photo)
    {
        if (photo is null || photo.Length == 0) return null;
        var dir = Path.Combine(env.WebRootPath, "uploads", "teachers");
        Directory.CreateDirectory(dir);
        var name = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
        await using var s = System.IO.File.Create(Path.Combine(dir, name));
        await photo.CopyToAsync(s);
        return $"/uploads/teachers/{name}";
    }

    private void DeletePhoto(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        var path = Path.Combine(env.WebRootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
    }
}
