using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shivakala.SqlServerMigrations.Migrations
{
    public partial class AddAboutPageContentSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublicDesignation",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicDesignationMarathi",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicExperience",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicExperienceMarathi",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowOnAboutPage",
                table: "Teachers",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "AboutPageSectionSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShowStatisticsSection = table.Column<bool>(type: "bit", nullable: false),
                    Stat1Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stat1Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stat1LabelMarathi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stat2Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stat2Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stat2LabelMarathi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stat3Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stat3Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stat3LabelMarathi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stat4Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stat4Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stat4LabelMarathi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressMarathi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MapEmbedUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AboutPageSectionSettings", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AboutPageSectionSettings");

            migrationBuilder.DropColumn(
                name: "PublicDesignation",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "PublicDesignationMarathi",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "PublicExperience",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "PublicExperienceMarathi",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "ShowOnAboutPage",
                table: "Teachers");
        }
    }
}
