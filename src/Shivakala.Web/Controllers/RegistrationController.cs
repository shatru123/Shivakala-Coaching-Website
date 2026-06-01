using Microsoft.AspNetCore.Mvc;
using Shivakala.Core.Services;
using Shivakala.Core.ViewModels;
using System.Globalization;

namespace Shivakala.Web.Controllers;

public sealed class RegistrationController(IRegistrationService registrationService, ILogger<RegistrationController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await registrationService.GetFormViewModelAsync(ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(RegistrationFormViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(await registrationService.GetFormViewModelAsync(ct) with { FullName = model.FullName, Mobile = model.Mobile, Email = model.Email, Standard = model.Standard, Subject = model.Subject, Address = model.Address, Board = model.Board, Medium = model.Medium, ParentName = model.ParentName });

        await registrationService.RegisterAsync(model, ct);
        var isMr = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "mr";
        TempData["SuccessMessage"] = isMr
            ? "नोंदणी यशस्वीरीत्या झाली! आम्ही लवकरच संपर्क करू."
            : "Registration submitted successfully! We will contact you soon.";
        logger.LogInformation("New registration: {Name} for Std {Standard}", model.FullName, model.Standard);
        return RedirectToAction(nameof(Index));
    }
}
