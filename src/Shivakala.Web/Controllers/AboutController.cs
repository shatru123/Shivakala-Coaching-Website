using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Shivakala.Core.Common;
using Shivakala.Core.ViewModels;

namespace Shivakala.Web.Controllers;

public sealed class AboutController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var isMarathi = CultureInfo.CurrentUICulture.IsMarathi();

        var model = new AboutPageViewModel
        {
            Seo = new SeoViewModel
            {
                Title = "About Shivakala Coaching Classes",
                Description = "Learn about Shivakala Coaching Classes, our academic vision, and our experienced faculty.",
                Keywords = "about Shivakala Coaching Classes, faculty, coaching institute Maharashtra"
            },
            Milestones =
            [
                new() { Icon = "fa-solid fa-school", Title = isMarathi ? "मजबूत शैक्षणिक पाया" : "Strong academic foundation", Description = isMarathi ? "स्थानिक विद्यार्थ्यांसाठी संकल्पनांवर आधारित प्रशिक्षण." : "Concept-driven coaching designed for local students with ambitious goals." },
                new() { Icon = "fa-solid fa-people-group", Title = isMarathi ? "पालक-सहभाग मॉडेल" : "Parent partnership model", Description = isMarathi ? "नियमित संवादामुळे विद्यार्थी, शिक्षक आणि पालक एकाच दिशेने काम करतात." : "Frequent communication aligns students, mentors, and parents on progress." },
                new() { Icon = "fa-solid fa-medal", Title = isMarathi ? "निकालकेंद्री संस्कृती" : "Results-led culture", Description = isMarathi ? "टेस्ट सीरिज, मार्गदर्शन आणि सातत्यपूर्ण फीडबॅक ही आमची वैशिष्ट्ये." : "Test series, mentoring, and fast feedback create measurable outcomes." }
            ],
            FacultyMembers =
            [
                new() { Name = "Prof. Shrikant Sir", Designation = isMarathi ? "संस्थापक आणि मुख्य मार्गदर्शक" : "Founder & Chief Mentor", Experience = isMarathi ? "15+ वर्षे अनुभव" : "15+ years of experience", Speciality = isMarathi ? "गणित, करिअर मार्गदर्शन" : "Mathematics, career guidance" },
                new() { Name = "Mrs. Kavita Ma'am", Designation = isMarathi ? "सायन्स विभाग प्रमुख" : "Science Department Lead", Experience = isMarathi ? "12+ वर्षे अनुभव" : "12+ years of experience", Speciality = isMarathi ? "सायन्स, प्रयोगशील अध्यापन" : "Science, experiential learning" },
                new() { Name = "Mr. Nilesh Sir", Designation = isMarathi ? "भाषा आणि चाचणी नियोजन" : "Language & Assessment Strategy", Experience = isMarathi ? "10+ वर्षे अनुभव" : "10+ years of experience", Speciality = isMarathi ? "इंग्रजी, लेखन कौशल्य" : "English, writing skills" }
            ]
        };

        return View(model);
    }
}
