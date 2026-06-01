using Microsoft.AspNetCore.Mvc;
using Shivakala.Core.ViewModels;
using Shivakala.Infrastructure.Repositories;

namespace Shivakala.Web.Controllers;

public sealed class GalleryController(IGalleryRepository galleryRepo) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? category, CancellationToken ct)
    {
        var items = await galleryRepo.GetActiveAsync(category, ct);
        var cats = await galleryRepo.GetCategoriesAsync(ct);
        var vm = new GalleryPageViewModel
        {
            Items = items.Select(g => new GalleryItemViewModel {
                Id=g.Id, Title=g.Title, ImageUrl=g.ImageUrl,
                Caption=g.Caption, Category=g.Category, DisplayOrder=g.DisplayOrder
            }).ToList(),
            Categories = cats,
            SelectedCategory = category,
            Seo = new() { Title = "Gallery | Shivakala Coaching Classes", Description = "Photos from events, classes and achievements at Shivakala Coaching Classes." }
        };
        return View(vm);
    }
}
