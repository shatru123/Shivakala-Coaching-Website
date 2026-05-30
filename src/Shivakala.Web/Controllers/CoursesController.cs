using Microsoft.AspNetCore.Mvc;
using Shivakala.Core.Services;
using Shivakala.Core.ViewModels;

namespace Shivakala.Web.Controllers;

public sealed class CoursesController(ICourseService courseService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewData["Seo"] = new SeoViewModel
        {
            Title = "Courses at Shivakala Coaching Classes",
            Description = "Explore foundation, SSC, mathematics, science, and scholarship coaching programs at Shivakala Coaching Classes.",
            Keywords = "Shivakala courses, SSC course, science maths coaching, scholarship classes"
        };

        return View(await courseService.GetCoursesAsync(cancellationToken));
    }
}
