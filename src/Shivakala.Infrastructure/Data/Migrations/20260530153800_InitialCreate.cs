using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Shivakala.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    TitleMarathi = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    DescriptionMarathi = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    Standard = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    DurationMonths = table.Column<int>(type: "INTEGER", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsFeatured = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Enquiries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Mobile = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 600, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enquiries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Mobile = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Standard = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "Description", "DescriptionMarathi", "DisplayOrder", "DurationMonths", "IsFeatured", "Slug", "Standard", "Title", "TitleMarathi" },
                values: new object[,]
                {
                    { 1, "Strong conceptual coaching for classes 8 to 10 with daily practice, tests, and mentorship.", "इयत्ता ८ वी ते १० वी साठी दैनंदिन सराव, चाचण्या आणि मार्गदर्शनासह मजबूत पाया घडवणारे प्रशिक्षण.", 1, 12, true, "foundation-batch", "8th - 10th", "Foundation Batch", "फाउंडेशन बॅच" },
                    { 2, "Exam-focused batch for SSC students with revision plans, paper solving, and result tracking.", "एसएससी विद्यार्थ्यांसाठी पुनरावृत्ती योजना, पेपर सोडवणे आणि निकाल विश्लेषणासह परीक्षा-केंद्रित बॅच.", 2, 10, true, "board-excellence", "10th SSC", "Board Excellence Program", "बोर्ड उत्कृष्टता कार्यक्रम" },
                    { 3, "Dedicated advanced coaching for Mathematics and Science with doubt-solving labs.", "गणित आणि विज्ञान विषयांसाठी विशेष शंका समाधान सत्रांसह प्रगत प्रशिक्षण.", 3, 8, true, "science-maths-mastery", "9th - 10th", "Science & Maths Mastery", "सायन्स आणि मॅथ्स मास्टरी" },
                    { 4, "Reasoning, language, and aptitude sessions tailored for scholarship aspirants.", "शिष्यवृत्ती विद्यार्थ्यांसाठी रिझनिंग, भाषा आणि अॅप्टिट्यूडचे नियोजित मार्गदर्शन.", 4, 6, false, "scholarship-prep", "5th - 8th", "Scholarship Preparation", "शिष्यवृत्ती तयारी" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Slug",
                table: "Courses",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Enquiries");

            migrationBuilder.DropTable(
                name: "Students");
        }
    }
}
