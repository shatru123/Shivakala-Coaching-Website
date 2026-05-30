using System.Globalization;
using Shivakala.Core.Common;
using Shivakala.Core.Services;
using Shivakala.Core.ViewModels;

namespace Shivakala.Infrastructure.Services;

public sealed class HomePageService(ICourseService courseService) : IHomePageService
{
    public async Task<HomePageViewModel> GetHomePageAsync(CancellationToken cancellationToken = default)
    {
        var isMarathi = CultureInfo.CurrentUICulture.IsMarathi();
        var featuredCourses = await courseService.GetFeaturedCoursesAsync(cancellationToken);

        return new HomePageViewModel
        {
            Seo = new SeoViewModel
            {
                Title = "Shivakala Coaching Classes | SSC, Foundation & Scholarship Coaching",
                Description = "Production-ready coaching website for Shivakala Coaching Classes with multilingual admissions and enquiry experience.",
                Keywords = "Shivakala Coaching Classes, SSC coaching, foundation batch, scholarship preparation, Marathi coaching website"
            },
            FeaturedCourses = featuredCourses,
            Statistics =
            [
                new() { Value = "12+", Label = isMarathi ? "वर्षांचा अनुभव" : "Years of experience" },
                new() { Value = "2,500+", Label = isMarathi ? "यशस्वी विद्यार्थी" : "Successful students" },
                new() { Value = "96%", Label = isMarathi ? "बोर्ड निकाल" : "Board success rate" },
                new() { Value = "24x7", Label = isMarathi ? "विद्यार्थी मार्गदर्शन" : "Student mentoring" }
            ],
            Highlights =
            [
                new() { Icon = "fa-solid fa-book-open-reader", Title = isMarathi ? "संकल्पनांवर भर" : "Concept-first teaching", Description = isMarathi ? "मूलभूत संकल्पना स्पष्ट करून दीर्घकालीन समज वाढवतो." : "We build deep understanding through concept clarity and structured revision." },
                new() { Icon = "fa-solid fa-chart-line", Title = isMarathi ? "नियमित प्रगती विश्लेषण" : "Regular performance tracking", Description = isMarathi ? "चाचण्या, विश्लेषण आणि पालक संवादामुळे सातत्य टिकते." : "Frequent tests, analytics, and parent updates keep performance improving." },
                new() { Icon = "fa-solid fa-user-group", Title = isMarathi ? "लहान बॅचेस" : "Focused small batches", Description = isMarathi ? "प्रत्येक विद्यार्थ्याकडे वैयक्तिक लक्ष देण्यासाठी नियोजित बॅच रचना." : "Smaller batch sizes ensure each student gets personalised guidance." }
            ],
            Results =
            [
                new() { Icon = "fa-solid fa-trophy", Title = isMarathi ? "SSC टॉपर परंपरा" : "Consistent SSC toppers", Description = isMarathi ? "गेल्या काही वर्षांत अनेक विद्यार्थ्यांनी 90% पेक्षा जास्त गुण मिळवले." : "Our students consistently secure 90%+ scores across SSC boards." },
                new() { Icon = "fa-solid fa-bullseye", Title = isMarathi ? "शिष्यवृत्ती यश" : "Scholarship success", Description = isMarathi ? "स्पर्धात्मक परीक्षांसाठी विशेष सराव आणि रणनीती." : "Targeted preparation helps scholarship aspirants compete confidently." },
                new() { Icon = "fa-solid fa-lightbulb", Title = isMarathi ? "दैनंदिन शंका समाधान" : "Daily doubt solving", Description = isMarathi ? "शंका राहू नयेत म्हणून वेगवेगळ्या सपोर्ट सत्रांची रचना." : "Dedicated doubt-solving sessions keep learning momentum strong." }
            ],
            Testimonials =
            [
                new() { StudentName = "Aarohi Patil", Achievement = isMarathi ? "SSC - 94.80%" : "SSC - 94.80%", Quote = isMarathi ? "शिवकला मधील चाचण्या आणि वैयक्तिक मार्गदर्शनामुळे माझा आत्मविश्वास खूप वाढला." : "The structured tests and personal mentoring at Shivakala boosted my confidence tremendously." },
                new() { StudentName = "Vedant Jadhav", Achievement = isMarathi ? "Scholarship Qualifier" : "Scholarship Qualifier", Quote = isMarathi ? "माझ्या रिझनिंग आणि गणितात झालेली प्रगती इथल्या सातत्यपूर्ण सरावामुळेच शक्य झाली." : "My reasoning and maths improved because of the disciplined practice routine here." },
                new() { StudentName = "Sakshi More", Achievement = isMarathi ? "Maths Topper" : "Maths Topper", Quote = isMarathi ? "प्रत्येक शंका संयमाने समजावून सांगणारी शिक्षकांची टीम ही आमची सर्वात मोठी ताकद आहे." : "The faculty patiently solved every doubt, and that became my biggest strength." }
            ],
            FacultyMembers =
            [
                new() { Name = "Prof. Shrikant Sir", Designation = isMarathi ? "संस्थापक आणि गणित मार्गदर्शक" : "Founder & Mathematics Mentor", Experience = isMarathi ? "15+ वर्षे" : "15+ years", Speciality = isMarathi ? "बोर्ड, स्कॉलरशिप, ऑलिंपियाड" : "Boards, scholarships, olympiads" },
                new() { Name = "Mrs. Kavita Ma'am", Designation = isMarathi ? "सायन्स तज्ज्ञ" : "Science Specialist", Experience = isMarathi ? "12+ वर्षे" : "12+ years", Speciality = isMarathi ? "प्रायोगिक संकल्पना आणि रिव्हिजन" : "Practical concepts and revision strategy" },
                new() { Name = "Mr. Nilesh Sir", Designation = isMarathi ? "इंग्रजी आणि टेस्ट स्ट्रॅटेजी" : "English & Test Strategy", Experience = isMarathi ? "10+ वर्षे" : "10+ years", Speciality = isMarathi ? "भाषिक कौशल्य आणि लेखन" : "Language skills and writing improvement" }
            ]
        };
    }
}
