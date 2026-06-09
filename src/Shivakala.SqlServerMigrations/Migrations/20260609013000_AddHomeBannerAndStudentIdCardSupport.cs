using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shivakala.SqlServerMigrations.Migrations
{
    public partial class AddHomeBannerAndStudentIdCardSupport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HeroBannerAltText",
                table: "HomePageSectionSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Shivakala Classes admissions banner");

            migrationBuilder.AddColumn<string>(
                name: "HeroBannerImageUrl",
                table: "HomePageSectionSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "/img/Banner.jpeg");

            migrationBuilder.AddColumn<bool>(
                name: "ShowTrendingBanner",
                table: "HomePageSectionSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TrendingAltText",
                table: "HomePageSectionSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Trending banner for Shivakala Coaching Classes");

            migrationBuilder.AddColumn<string>(
                name: "TrendingDescription",
                table: "HomePageSectionSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Highlight important announcements, batches, offers, or events right from the admin panel.");

            migrationBuilder.AddColumn<string>(
                name: "TrendingDescriptionMarathi",
                table: "HomePageSectionSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "महत्त्वाच्या घोषणा, बॅचेस, ऑफर्स किंवा इव्हेंट्स अॅडमिन पॅनलमधून लगेच दाखवा.");

            migrationBuilder.AddColumn<string>(
                name: "TrendingEyebrow",
                table: "HomePageSectionSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Trending Now");

            migrationBuilder.AddColumn<string>(
                name: "TrendingEyebrowMarathi",
                table: "HomePageSectionSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "नवीन अपडेट");

            migrationBuilder.AddColumn<string>(
                name: "TrendingImageUrl",
                table: "HomePageSectionSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "/img/Banner.jpeg");

            migrationBuilder.AddColumn<string>(
                name: "TrendingLinkText",
                table: "HomePageSectionSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Explore Now");

            migrationBuilder.AddColumn<string>(
                name: "TrendingLinkTextMarathi",
                table: "HomePageSectionSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "अधिक जाणून घ्या");

            migrationBuilder.AddColumn<string>(
                name: "TrendingLinkUrl",
                table: "HomePageSectionSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "/registration");

            migrationBuilder.AddColumn<string>(
                name: "TrendingTitle",
                table: "HomePageSectionSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Admissions open for the new academic year");

            migrationBuilder.AddColumn<string>(
                name: "TrendingTitleMarathi",
                table: "HomePageSectionSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "नवीन शैक्षणिक वर्षासाठी प्रवेश सुरू");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeroBannerAltText",
                table: "HomePageSectionSettings");

            migrationBuilder.DropColumn(
                name: "HeroBannerImageUrl",
                table: "HomePageSectionSettings");

            migrationBuilder.DropColumn(
                name: "ShowTrendingBanner",
                table: "HomePageSectionSettings");

            migrationBuilder.DropColumn(
                name: "TrendingAltText",
                table: "HomePageSectionSettings");

            migrationBuilder.DropColumn(
                name: "TrendingDescription",
                table: "HomePageSectionSettings");

            migrationBuilder.DropColumn(
                name: "TrendingDescriptionMarathi",
                table: "HomePageSectionSettings");

            migrationBuilder.DropColumn(
                name: "TrendingEyebrow",
                table: "HomePageSectionSettings");

            migrationBuilder.DropColumn(
                name: "TrendingEyebrowMarathi",
                table: "HomePageSectionSettings");

            migrationBuilder.DropColumn(
                name: "TrendingImageUrl",
                table: "HomePageSectionSettings");

            migrationBuilder.DropColumn(
                name: "TrendingLinkText",
                table: "HomePageSectionSettings");

            migrationBuilder.DropColumn(
                name: "TrendingLinkTextMarathi",
                table: "HomePageSectionSettings");

            migrationBuilder.DropColumn(
                name: "TrendingLinkUrl",
                table: "HomePageSectionSettings");

            migrationBuilder.DropColumn(
                name: "TrendingTitle",
                table: "HomePageSectionSettings");

            migrationBuilder.DropColumn(
                name: "TrendingTitleMarathi",
                table: "HomePageSectionSettings");
        }
    }
}
