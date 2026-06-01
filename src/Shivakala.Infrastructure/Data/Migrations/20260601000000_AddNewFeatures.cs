using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shivakala.Infrastructure.Data.Migrations
{
    [DbContext(typeof(ShivakalaDbContext))]
    [Migration("20260601000000_AddNewFeatures")]
    public partial class AddNewFeatures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Alter Students: add new columns
            migrationBuilder.AddColumn<string>("ParentName", "Students", "TEXT", maxLength: 120, nullable: true);
            migrationBuilder.AddColumn<string>("Board", "Students", "TEXT", maxLength: 80, nullable: true);
            migrationBuilder.AddColumn<string>("Medium", "Students", "TEXT", maxLength: 60, nullable: true);
            migrationBuilder.AddColumn<string>("Status", "Students", "TEXT", maxLength: 40, nullable: false, defaultValue: "Pending");
            migrationBuilder.AddColumn<string>("AdminNotes", "Students", "TEXT", maxLength: 500, nullable: true);

            // Alter Enquiries: add new columns
            migrationBuilder.AddColumn<string>("Email", "Enquiries", "TEXT", maxLength: 150, nullable: true);
            migrationBuilder.AddColumn<string>("ClassInterested", "Enquiries", "TEXT", maxLength: 40, nullable: true);
            migrationBuilder.AddColumn<bool>("IsRead", "Enquiries", "INTEGER", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<string>("AdminReply", "Enquiries", "TEXT", maxLength: 500, nullable: true);

            // Notices
            migrationBuilder.CreateTable("Notices", table => new
            {
                Id            = table.Column<int>("INTEGER",nullable:false).Annotation("Sqlite:Autoincrement",true),
                Title         = table.Column<string>("TEXT",maxLength:200,nullable:false),
                TitleMarathi  = table.Column<string>("TEXT",maxLength:200,nullable:false),
                Body          = table.Column<string>("TEXT",maxLength:2000,nullable:false),
                BodyMarathi   = table.Column<string>("TEXT",maxLength:2000,nullable:false),
                Category      = table.Column<string>("TEXT",maxLength:60,nullable:false,defaultValue:"General"),
                IsPinned      = table.Column<bool>("INTEGER",nullable:false),
                IsActive      = table.Column<bool>("INTEGER",nullable:false,defaultValue:true),
                PublishedDate = table.Column<DateTime>("TEXT",nullable:false),
                CreatedDate   = table.Column<DateTime>("TEXT",nullable:false)
            }, constraints: t => t.PrimaryKey("PK_Notices", x => x.Id));

            // TestResults
            migrationBuilder.CreateTable("TestResults", table => new
            {
                Id          = table.Column<int>("INTEGER",nullable:false).Annotation("Sqlite:Autoincrement",true),
                StudentName = table.Column<string>("TEXT",maxLength:120,nullable:false),
                Standard    = table.Column<string>("TEXT",maxLength:40,nullable:false),
                Subject     = table.Column<string>("TEXT",maxLength:80,nullable:false),
                Score       = table.Column<int>("INTEGER",nullable:false),
                TotalMarks  = table.Column<int>("INTEGER",nullable:false),
                Rank        = table.Column<int>("INTEGER",nullable:false),
                Grade       = table.Column<string>("TEXT",maxLength:10,nullable:true),
                Remarks     = table.Column<string>("TEXT",maxLength:300,nullable:true),
                TestDate    = table.Column<DateTime>("TEXT",nullable:false),
                TestTitle   = table.Column<string>("TEXT",maxLength:200,nullable:false),
                CreatedDate = table.Column<DateTime>("TEXT",nullable:false)
            }, constraints: t => t.PrimaryKey("PK_TestResults", x => x.Id));

            // StudyMaterials
            migrationBuilder.CreateTable("StudyMaterials", table => new
            {
                Id             = table.Column<int>("INTEGER",nullable:false).Annotation("Sqlite:Autoincrement",true),
                Title          = table.Column<string>("TEXT",maxLength:200,nullable:false),
                TitleMarathi   = table.Column<string>("TEXT",maxLength:200,nullable:false),
                FileUrl        = table.Column<string>("TEXT",maxLength:400,nullable:false),
                Standard       = table.Column<string>("TEXT",maxLength:40,nullable:false),
                Subject        = table.Column<string>("TEXT",maxLength:80,nullable:false),
                MaterialType   = table.Column<string>("TEXT",maxLength:40,nullable:false,defaultValue:"QuestionPaper"),
                FileSizeBytes  = table.Column<long>("INTEGER",nullable:false),
                IsActive       = table.Column<bool>("INTEGER",nullable:false,defaultValue:true),
                UploadedDate   = table.Column<DateTime>("TEXT",nullable:false)
            }, constraints: t => t.PrimaryKey("PK_StudyMaterials", x => x.Id));

            // GalleryItems
            migrationBuilder.CreateTable("GalleryItems", table => new
            {
                Id           = table.Column<int>("INTEGER",nullable:false).Annotation("Sqlite:Autoincrement",true),
                Title        = table.Column<string>("TEXT",maxLength:150,nullable:false),
                ImageUrl     = table.Column<string>("TEXT",maxLength:400,nullable:false),
                Caption      = table.Column<string>("TEXT",maxLength:300,nullable:true),
                Category     = table.Column<string>("TEXT",maxLength:60,nullable:false,defaultValue:"General"),
                DisplayOrder = table.Column<int>("INTEGER",nullable:false),
                IsActive     = table.Column<bool>("INTEGER",nullable:false,defaultValue:true),
                CreatedDate  = table.Column<DateTime>("TEXT",nullable:false)
            }, constraints: t => t.PrimaryKey("PK_GalleryItems", x => x.Id));

            // Testimonials
            migrationBuilder.CreateTable("Testimonials", table => new
            {
                Id            = table.Column<int>("INTEGER",nullable:false).Annotation("Sqlite:Autoincrement",true),
                Name          = table.Column<string>("TEXT",maxLength:120,nullable:false),
                Role          = table.Column<string>("TEXT",maxLength:120,nullable:false),
                Quote         = table.Column<string>("TEXT",maxLength:600,nullable:false),
                QuoteMarathi  = table.Column<string>("TEXT",maxLength:600,nullable:true),
                Rating        = table.Column<int>("INTEGER",nullable:false,defaultValue:5),
                IsApproved    = table.Column<bool>("INTEGER",nullable:false),
                IsFeatured    = table.Column<bool>("INTEGER",nullable:false),
                CreatedDate   = table.Column<DateTime>("TEXT",nullable:false)
            }, constraints: t => t.PrimaryKey("PK_Testimonials", x => x.Id));

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("Notices");
            migrationBuilder.DropTable("TestResults");
            migrationBuilder.DropTable("StudyMaterials");
            migrationBuilder.DropTable("GalleryItems");
            migrationBuilder.DropTable("Testimonials");
            migrationBuilder.DropColumn("ParentName","Students");
            migrationBuilder.DropColumn("Board","Students");
            migrationBuilder.DropColumn("Medium","Students");
            migrationBuilder.DropColumn("Status","Students");
            migrationBuilder.DropColumn("AdminNotes","Students");
            migrationBuilder.DropColumn("Email","Enquiries");
            migrationBuilder.DropColumn("ClassInterested","Enquiries");
            migrationBuilder.DropColumn("IsRead","Enquiries");
            migrationBuilder.DropColumn("AdminReply","Enquiries");
        }
    }
}
