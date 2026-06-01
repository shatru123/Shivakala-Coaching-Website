using Microsoft.AspNetCore.Mvc;
using Shivakala.Core.ViewModels;
using Shivakala.Infrastructure.Repositories;

namespace Shivakala.Web.Controllers;

public sealed class StudyMaterialController(IStudyMaterialRepository materialRepo) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? standard, string? subject, string? type, CancellationToken ct)
    {
        var materials = await materialRepo.GetActiveAsync(standard, subject, type, ct);
        var vm = new StudyMaterialsPageViewModel
        {
            Materials = materials.Select(m => new StudyMaterialViewModel {
                Id=m.Id, Title=m.Title, TitleMarathi=m.TitleMarathi, FileUrl=m.FileUrl,
                Standard=m.Standard, Subject=m.Subject, MaterialType=m.MaterialType,
                FileSizeBytes=m.FileSizeBytes, IsActive=m.IsActive, UploadedDate=m.UploadedDate
            }).ToList(),
            SelectedStandard = standard,
            SelectedSubject = subject,
            SelectedType = type,
            Seo = new() { Title = "Study Materials | Shivakala Coaching Classes", Description = "Download question papers, answer sheets and study notes for all standards." }
        };
        return View(vm);
    }
}
