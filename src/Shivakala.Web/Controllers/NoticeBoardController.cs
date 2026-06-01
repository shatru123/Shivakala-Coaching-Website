using Microsoft.AspNetCore.Mvc;
using Shivakala.Core.ViewModels;
using Shivakala.Infrastructure.Repositories;
using System.Globalization;

namespace Shivakala.Web.Controllers;

public sealed class NoticeBoardController(INoticeRepository noticeRepo) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? category, CancellationToken ct)
    {
        var all = await noticeRepo.GetActiveAsync(category, ct);
        var isMr = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "mr";
        var vm = new NoticeBoardViewModel
        {
            Pinned = all.Where(n => n.IsPinned).Select(n => Map(n, isMr)).ToList(),
            All = all.Where(n => !n.IsPinned).Select(n => Map(n, isMr)).ToList(),
            SelectedCategory = category,
            Seo = new() { Title = "Notice Board | Shivakala Coaching Classes", Description = "Latest notices, exam schedules and announcements from Shivakala Coaching Classes." }
        };
        return View(vm);
    }

    private static NoticeViewModel Map(Core.Entities.Notice n, bool isMr) => new()
    {
        Id = n.Id, Title = isMr ? n.TitleMarathi : n.Title,
        TitleMarathi = n.TitleMarathi, Body = isMr ? n.BodyMarathi : n.Body,
        BodyMarathi = n.BodyMarathi, Category = n.Category,
        IsPinned = n.IsPinned, IsActive = n.IsActive, PublishedDate = n.PublishedDate
    };
}
