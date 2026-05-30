using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shivakala.Core.Entities;

namespace Shivakala.Infrastructure.Data.Configurations;

public sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");
        builder.Property(x => x.Slug).IsRequired().HasMaxLength(80);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(120);
        builder.Property(x => x.TitleMarathi).IsRequired().HasMaxLength(120);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(400);
        builder.Property(x => x.DescriptionMarathi).IsRequired().HasMaxLength(400);
        builder.Property(x => x.Standard).IsRequired().HasMaxLength(80);
        builder.HasIndex(x => x.Slug).IsUnique();

        builder.HasData(
            new Course
            {
                Id = 1,
                Slug = "foundation-batch",
                Title = "Foundation Batch",
                TitleMarathi = "फाउंडेशन बॅच",
                Description = "Strong conceptual coaching for classes 8 to 10 with daily practice, tests, and mentorship.",
                DescriptionMarathi = "इयत्ता ८ वी ते १० वी साठी दैनंदिन सराव, चाचण्या आणि मार्गदर्शनासह मजबूत पाया घडवणारे प्रशिक्षण.",
                Standard = "8th - 10th",
                DurationMonths = 12,
                DisplayOrder = 1,
                IsFeatured = true
            },
            new Course
            {
                Id = 2,
                Slug = "board-excellence",
                Title = "Board Excellence Program",
                TitleMarathi = "बोर्ड उत्कृष्टता कार्यक्रम",
                Description = "Exam-focused batch for SSC students with revision plans, paper solving, and result tracking.",
                DescriptionMarathi = "एसएससी विद्यार्थ्यांसाठी पुनरावृत्ती योजना, पेपर सोडवणे आणि निकाल विश्लेषणासह परीक्षा-केंद्रित बॅच.",
                Standard = "10th SSC",
                DurationMonths = 10,
                DisplayOrder = 2,
                IsFeatured = true
            },
            new Course
            {
                Id = 3,
                Slug = "science-maths-mastery",
                Title = "Science & Maths Mastery",
                TitleMarathi = "सायन्स आणि मॅथ्स मास्टरी",
                Description = "Dedicated advanced coaching for Mathematics and Science with doubt-solving labs.",
                DescriptionMarathi = "गणित आणि विज्ञान विषयांसाठी विशेष शंका समाधान सत्रांसह प्रगत प्रशिक्षण.",
                Standard = "9th - 10th",
                DurationMonths = 8,
                DisplayOrder = 3,
                IsFeatured = true
            },
            new Course
            {
                Id = 4,
                Slug = "scholarship-prep",
                Title = "Scholarship Preparation",
                TitleMarathi = "शिष्यवृत्ती तयारी",
                Description = "Reasoning, language, and aptitude sessions tailored for scholarship aspirants.",
                DescriptionMarathi = "शिष्यवृत्ती विद्यार्थ्यांसाठी रिझनिंग, भाषा आणि अॅप्टिट्यूडचे नियोजित मार्गदर्शन.",
                Standard = "5th - 8th",
                DurationMonths = 6,
                DisplayOrder = 4,
                IsFeatured = false
            });
    }
}
