using Microsoft.AspNetCore.Mvc;
using Shivakala.Core.ViewModels;
using Shivakala.Infrastructure.Repositories;
using System.Globalization;

namespace Shivakala.Web.Controllers;

public sealed class TestimonialsController(ITestimonialRepository testimonialRepo) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var isMr = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "mr";
        var items = await testimonialRepo.GetApprovedAsync(ct: ct);
        var vms = items.Select(t => new TestimonialViewModel {
            StudentName = t.Name,
            Achievement = t.Role,
            Quote = isMr && !string.IsNullOrWhiteSpace(t.QuoteMarathi) ? t.QuoteMarathi : t.Quote,
            Rating = t.Rating
        }).ToList();
        ViewData["Title"] = "Testimonials | Shivakala Coaching Classes";
        return View(vms);
    }

    // Public submit a testimonial
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(string name, string role, string quote, int rating, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(quote))
        {
            await testimonialRepo.AddAsync(new Core.Entities.Testimonial {
                Name = name, Role = role ?? "Parent / Student", Quote = quote, Rating = rating,
                IsApproved = false, IsFeatured = false
            }, ct);
        }
        TempData["SuccessMessage"] = "Thank you! Your review has been submitted for approval.";
        return RedirectToAction(nameof(Index));
    }
}
