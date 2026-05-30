using Microsoft.AspNetCore.Mvc;
using Shivakala.Core.Services;
using Shivakala.Core.ViewModels;

namespace Shivakala.Web.Controllers;

public sealed class EnquiryController(
    IEnquiryService enquiryService,
    ILogger<EnquiryController> logger) : Controller
{
    [HttpGet]
    public IActionResult Index() => View(CreateModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(EnquiryFormViewModel model, CancellationToken cancellationToken)
    {
        model.Seo = CreateSeo();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await enquiryService.SubmitEnquiryAsync(model, cancellationToken);
        TempData["SuccessMessage"] = "Enquiry submitted successfully.";
        logger.LogInformation("Enquiry form submitted by {Name}", model.Name);

        return RedirectToAction(nameof(Index));
    }

    private static EnquiryFormViewModel CreateModel() => new() { Seo = CreateSeo() };

    private static SeoViewModel CreateSeo() => new()
    {
        Title = "Quick Enquiry | Shivakala Coaching Classes",
        Description = "Send your enquiry to Shivakala Coaching Classes for admissions, batches, timings, or course guidance.",
        Keywords = "Shivakala enquiry, coaching enquiry form, course enquiry"
    };
}
