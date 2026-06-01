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
        {
            var viewModel = await registrationService.GetFormViewModelAsync(ct);
            viewModel.FullName = model.FullName;
            viewModel.Mobile = model.Mobile;
            viewModel.Email = model.Email;
            viewModel.Standard = model.Standard;
            viewModel.Subject = model.Subject;
            viewModel.Address = model.Address;
            viewModel.Board = model.Board;
            viewModel.Medium = model.Medium;
            viewModel.ParentName = model.ParentName;
            return View(viewModel);
        }

        await registrationService.RegisterAsync(model, ct);
        var isMr = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "mr";
        TempData["SuccessMessage"] = isMr
            ? "नोंदणी यशस्वीरीत्या झाली! आम्ही लवकरच संपर्क करू."
            : "Registration submitted successfully! We will contact you soon.";
        logger.LogInformation("New registration: {Name} for Std {Standard}", model.FullName, model.Standard);
        return RedirectToAction(nameof(Index));
    }
}
