using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shivakala.Infrastructure.Data.Migrations
{
    public partial class AddNewFeatures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Alter Students: add new columns
            migrationBuilder.AddColumn<string>("ParentName","Students","TEXT",120,nullable:true);
            migrationBuilder.AddColumn<string>("Board","Students","TEXT",80,nullable:true);
            migrationBuilder.AddColumn<string>("Medium","Students","TEXT",60,nullable:true);
            migrationBuilder.AddColumn<string>("Status","Students","TEXT",40,nullable:false,defaultValue:"Pending");
            migrationBuilder.AddColumn<string>("AdminNotes","Students","TEXT",500,nullable:true);

            // Alter Enquiries: add new columns
            migrationBuilder.AddColumn<string>("Email","Enquiries","TEXT",150,nullable:true);
            migrationBuilder.AddColumn<string>("ClassInterested","Enquiries","TEXT",40,nullable:true);
            migrationBuilder.AddColumn<bool>("IsRead","Enquiries","INTEGER",nullable:false,defaultValue:false);
            migrationBuilder.AddColumn<string>("AdminReply","Enquiries","TEXT",500,nullable:true);

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

            // Seed notices
            migrationBuilder.InsertData("Notices",
                new[]{"Id","Title","TitleMarathi","Body","BodyMarathi","Category","IsPinned","IsActive","PublishedDate","CreatedDate"},
                new object[]{1,"Admissions Open 2026-27","प्रवेश सुरू आहेत २०२६-२७",
                    "Admissions for the academic year 2026-27 are now open for KG1 to 10th Standard. Limited seats available. Register online or visit us at Chikhali.",
                    "शैक्षणिक वर्ष २०२६-२७ साठी KG1 ते 10वी इयत्तेसाठी प्रवेश सुरू झाले आहेत. मर्यादित जागा उपलब्ध. ऑनलाइन नोंदणी करा किंवा चिखलीला भेट द्या.",
                    "Admission",true,true,new DateTime(2026,6,1),new DateTime(2026,6,1)});

            migrationBuilder.InsertData("Notices",
                new[]{"Id","Title","TitleMarathi","Body","BodyMarathi","Category","IsPinned","IsActive","PublishedDate","CreatedDate"},
                new object[]{2,"Weekly Test Every Saturday","दर शनिवारी साप्ताहिक चाचणी",
                    "Weekly tests are conducted every Saturday. Students must bring their question papers and answer sheets. Results and merit list are published every Sunday.",
                    "दर शनिवारी साप्ताहिक चाचण्या घेतल्या जातात. विद्यार्थ्यांनी प्रश्नपत्रिका व उत्तरपत्रिका आणाव्यात. दर रविवारी निकाल व गुणवत्ता यादी प्रकाशित होते.",
                    "Exam",false,true,new DateTime(2026,6,1),new DateTime(2026,6,1)});

            // Seed testimonials
            migrationBuilder.InsertData("Testimonials",
                new[]{"Id","Name","Role","Quote","QuoteMarathi","Rating","IsApproved","IsFeatured","CreatedDate"},
                new object[]{1,"Priya Sharma","Parent of 10th Std Student",
                    "Shivakala Coaching Classes transformed my daughter's approach to Mathematics. Her marks improved from 65% to 92% in just one year!",
                    "शिवकला क्लासेसने माझ्या मुलीचा गणिताकडे पाहण्याचा दृष्टिकोन बदलला. एका वर्षात तिचे गुण ६५% वरून ९२% झाले!",
                    5,true,true,new DateTime(2026,5,1)});

            migrationBuilder.InsertData("Testimonials",
                new[]{"Id","Name","Role","Quote","QuoteMarathi","Rating","IsApproved","IsFeatured","CreatedDate"},
                new object[]{2,"Rahul Patil","9th Std Student",
                    "The weekly tests are amazing! I love how we get our results and merit rank every Sunday. It keeps me motivated to study harder.",
                    "साप्ताहिक चाचण्या खूप छान आहेत! दर रविवारी निकाल व गुणवत्ता यादी मिळते. याने मला जास्त अभ्यास करण्याची प्रेरणा मिळते.",
                    5,true,true,new DateTime(2026,5,5)});

            migrationBuilder.InsertData("Testimonials",
                new[]{"Id","Name","Role","Quote","QuoteMarathi","Rating","IsApproved","IsFeatured","CreatedDate"},
                new object[]{3,"Anjali More","Parent of 7th Std Student",
                    "The personal attention given by teachers is exceptional. My son who was struggling with Science now loves the subject!",
                    "शिक्षकांचे वैयक्तिक लक्ष अतुलनीय आहे. विज्ञानात कठीण वाटणाऱ्या माझ्या मुलाला आता तो विषय आवडतो!",
                    5,true,false,new DateTime(2026,5,10)});
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
