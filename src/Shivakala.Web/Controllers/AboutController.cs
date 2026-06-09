using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Shivakala.Core.Common;
using Shivakala.Core.Services;

namespace Shivakala.Web.Controllers;

public sealed class AboutController(IAboutPageService aboutPageService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await aboutPageService.GetAboutPageAsync(cancellationToken);
        return View(model);
    }
}
