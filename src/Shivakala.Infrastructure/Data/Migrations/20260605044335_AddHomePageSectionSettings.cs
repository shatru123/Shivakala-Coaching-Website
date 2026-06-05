using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shivakala.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHomePageSectionSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HomePageSectionSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ShowStatisticsSection = table.Column<bool>(type: "INTEGER", nullable: false),
                    Stat1Value = table.Column<string>(type: "TEXT", nullable: false),
                    Stat1Label = table.Column<string>(type: "TEXT", nullable: false),
                    Stat1LabelMarathi = table.Column<string>(type: "TEXT", nullable: false),
                    Stat2Value = table.Column<string>(type: "TEXT", nullable: false),
                    Stat2Label = table.Column<string>(type: "TEXT", nullable: false),
                    Stat2LabelMarathi = table.Column<string>(type: "TEXT", nullable: false),
                    Stat3Value = table.Column<string>(type: "TEXT", nullable: false),
                    Stat3Label = table.Column<string>(type: "TEXT", nullable: false),
                    Stat3LabelMarathi = table.Column<string>(type: "TEXT", nullable: false),
                    Stat4Value = table.Column<string>(type: "TEXT", nullable: false),
                    Stat4Label = table.Column<string>(type: "TEXT", nullable: false),
                    Stat4LabelMarathi = table.Column<string>(type: "TEXT", nullable: false),
                    ShowTestimonialsSection = table.Column<bool>(type: "INTEGER", nullable: false),
                    TestimonialsEyebrow = table.Column<string>(type: "TEXT", nullable: false),
                    TestimonialsEyebrowMarathi = table.Column<string>(type: "TEXT", nullable: false),
                    TestimonialsTitle = table.Column<string>(type: "TEXT", nullable: false),
                    TestimonialsTitleMarathi = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomePageSectionSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HomePageSectionSettings");
        }
    }
}
