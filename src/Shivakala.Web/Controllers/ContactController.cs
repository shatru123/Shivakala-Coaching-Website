using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Shivakala.Core.Common;
using Shivakala.Core.ViewModels;

namespace Shivakala.Web.Controllers;

public sealed class ContactController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var isMarathi = CultureInfo.CurrentUICulture.IsMarathi();

        return View(new ContactPageViewModel
        {
            Seo = new SeoViewModel
            {
                Title = "Contact Shivakala Coaching Classes",
                Description = "Contact Shivakala Coaching Classes for admissions, subject counselling, batch timings, and location details.",
                Keywords = "contact Shivakala Coaching Classes, coaching phone number, classes contact"
            },
            Phone = "+91 98765 43210",
            Email = "admissions@shivakalaclasses.in",
            Address = isMarathi ? "शिवकला कोचिंग क्लासेस, महाराष्ट्र" : "Shivakala Coaching Classes, Maharashtra",
            WhatsAppNumber = "919876543210",
            MapQuery = "Shivakala Coaching Classes Maharashtra"
        });
    }
}
