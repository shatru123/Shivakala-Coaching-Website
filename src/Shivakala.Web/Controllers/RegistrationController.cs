using Microsoft.AspNetCore.Mvc;
using Shivakala.Core.Services;
using Shivakala.Core.ViewModels;

namespace Shivakala.Web.Controllers;

public sealed class RegistrationController(
    IRegistrationService registrationService,
    ILogger<RegistrationController> logger) : Controller
{
    [HttpGet]
    public IActionResult Index() => View(CreateModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(RegistrationFormViewModel model, CancellationToken cancellationToken)
    {
        model.Seo = CreateSeo();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await registrationService.RegisterStudentAsync(model, cancellationToken);
        TempData["SuccessMessage"] = "Registration submitted successfully.";
        logger.LogInformation("Registration form submitted by {Name}", model.FullName);

        return RedirectToAction(nameof(Index));
    }

    private static RegistrationFormViewModel CreateModel() => new() { Seo = CreateSeo() };

    private static SeoViewModel CreateSeo() => new()
    {
        Title = "Student Registration | Shivakala Coaching Classes",
        Description = "Register for coaching admissions at Shivakala Coaching Classes through our secure online form.",
        Keywords = "Shivakala registration, coaching admission form, student registration"
    };
}
