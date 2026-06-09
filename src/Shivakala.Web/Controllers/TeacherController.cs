using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;
using Shivakala.Core.Services;
using Shivakala.Infrastructure.Data;

namespace Shivakala.Web.Controllers;

[Authorize(Roles = "Admin"), Route("admin/teachers")]
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
        try
        {
            ViewBag.PortalUsernames = await db.AppUsers
                .Where(u => u.Role == "Teacher" && u.TeacherId != null)
                .ToDictionaryAsync(u => u.TeacherId!.Value, u => u.Username, ct);
            await PopulateSchemaWarningAsync(ct);
            return View(await repo.GetAllAsync(ct));
        }
        catch
        {
            ViewBag.PortalUsernames = new Dictionary<int, string>();
            ViewBag.PageLoadWarning = "Teacher data is temporarily unavailable. The page is running in safe mode while production data access is being stabilized.";
            return View(Array.Empty<Teacher>());
        }
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        await PopulateSchemaWarningAsync(ct);
        return View("Form", new Teacher { FullName = "", Mobile = "" });
    }

    [HttpPost("create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Teacher model, IFormFile? photo, string? portalUsername, string? portalPassword, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PopulateSchemaWarningAsync(ct);
            return View("Form", model);
        }

        var supportsAboutPageFields = await TeacherSchemaCompatibility.SupportsAboutPageFieldsAsync(db, ct);
        model.PhotoUrl = await SavePhotoAsync(photo);
        await repo.AddAsync(model, ct);
        var portalUser = await portalUsers.EnsureTeacherAccountAsync(model.Id, portalUsername, portalPassword, ct);
        await audit.LogAsync("Created", "Teacher", model.Id, null,
            $"{{Name:{model.FullName}}}", User.Identity?.Name, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        TempData["SuccessMessage"] = $"Teacher added. Portal login — username: {portalUser.Username}, password: {(string.IsNullOrWhiteSpace(portalPassword) ? "last 4 digits of mobile" : "(as set)")}.";
        if (!supportsAboutPageFields)
            TempData["WarningMessage"] = "Teacher saved, but About page display fields will start saving after the production teacher migration finishes.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id}/edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        try
        {
            var t = await repo.GetByIdAsync(id, ct);
            if (t is null) return NotFound();
            await PopulateSchemaWarningAsync(ct);
            return View("Form", t);
        }
        catch
        {
            TempData["WarningMessage"] = "Teacher details could not be loaded right now.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("{id}/edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Teacher model, IFormFile? photo, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PopulateSchemaWarningAsync(ct);
            return View("Form", model);
        }
        var existing = await repo.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();
        var supportsAboutPageFields = await TeacherSchemaCompatibility.SupportsAboutPageFieldsAsync(db, ct);

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
        existing.ShowOnAboutPage = model.ShowOnAboutPage;
        existing.PublicDesignation = model.PublicDesignation;
        existing.PublicDesignationMarathi = model.PublicDesignationMarathi;
        existing.PublicExperience = model.PublicExperience;
        existing.PublicExperienceMarathi = model.PublicExperienceMarathi;
        existing.AdminNotes = model.AdminNotes;

        await repo.UpdateAsync(existing, ct);
        await audit.LogAsync("Updated", "Teacher", id, null, null, User.Identity?.Name,
            HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        TempData["SuccessMessage"] = "Teacher updated.";
        if (!supportsAboutPageFields)
            TempData["WarningMessage"] = "Teacher updated, but About page display fields will start saving after the production teacher migration finishes.";
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

    private async Task PopulateSchemaWarningAsync(CancellationToken ct)
    {
        try
        {
            if (!await TeacherSchemaCompatibility.SupportsAboutPageFieldsAsync(db, ct))
                ViewBag.SchemaWarning = "Teacher management is running in compatibility mode. Core teacher details still work, but About page display fields will start saving after the production teacher migration finishes.";
        }
        catch
        {
            ViewBag.SchemaWarning = "About page teacher fields are temporarily unavailable, but you can still manage core teacher details.";
        }
    }
}
