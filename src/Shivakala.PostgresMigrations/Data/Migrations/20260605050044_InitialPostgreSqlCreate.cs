using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Shivakala.PostgresMigrations.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgreSqlCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: true),
                    Mobile = table.Column<string>(type: "text", nullable: true),
                    PhotoUrl = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordResetToken = table.Column<string>(type: "text", nullable: true),
                    PasswordResetExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TeacherId = table.Column<int>(type: "integer", nullable: true),
                    StudentId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Action = table.Column<string>(type: "text", nullable: false),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<int>(type: "integer", nullable: true),
                    OldValues = table.Column<string>(type: "text", nullable: true),
                    NewValues = table.Column<string>(type: "text", nullable: true),
                    PerformedByUsername = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Batches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Standard = table.Column<string>(type: "text", nullable: false),
                    Medium = table.Column<string>(type: "text", nullable: true),
                    MaxStrength = table.Column<int>(type: "integer", nullable: false),
                    Room = table.Column<string>(type: "text", nullable: true),
                    TimingSlot = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AcademicYear = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Batches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Slug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    TitleMarathi = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    DescriptionMarathi = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    Standard = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    DurationMonths = table.Column<int>(type: "integer", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Enquiries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Mobile = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    ClassInterested = table.Column<string>(type: "text", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    AdminReply = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enquiries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeeStructures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Standard = table.Column<string>(type: "text", nullable: false),
                    FeeType = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    AcademicYear = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeStructures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GalleryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    Caption = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "text", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HomePageSectionSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShowStatisticsSection = table.Column<bool>(type: "boolean", nullable: false),
                    Stat1Value = table.Column<string>(type: "text", nullable: false),
                    Stat1Label = table.Column<string>(type: "text", nullable: false),
                    Stat1LabelMarathi = table.Column<string>(type: "text", nullable: false),
                    Stat2Value = table.Column<string>(type: "text", nullable: false),
                    Stat2Label = table.Column<string>(type: "text", nullable: false),
                    Stat2LabelMarathi = table.Column<string>(type: "text", nullable: false),
                    Stat3Value = table.Column<string>(type: "text", nullable: false),
                    Stat3Label = table.Column<string>(type: "text", nullable: false),
                    Stat3LabelMarathi = table.Column<string>(type: "text", nullable: false),
                    Stat4Value = table.Column<string>(type: "text", nullable: false),
                    Stat4Label = table.Column<string>(type: "text", nullable: false),
                    Stat4LabelMarathi = table.Column<string>(type: "text", nullable: false),
                    ShowTestimonialsSection = table.Column<bool>(type: "boolean", nullable: false),
                    TestimonialsEyebrow = table.Column<string>(type: "text", nullable: false),
                    TestimonialsEyebrowMarathi = table.Column<string>(type: "text", nullable: false),
                    TestimonialsTitle = table.Column<string>(type: "text", nullable: false),
                    TestimonialsTitleMarathi = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomePageSectionSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    TitleMarathi = table.Column<string>(type: "text", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    BodyMarathi = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    IsPinned = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Channel = table.Column<string>(type: "text", nullable: false),
                    Audience = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    TemplateKey = table.Column<string>(type: "text", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "text", nullable: true),
                    SentByUserId = table.Column<int>(type: "integer", nullable: true),
                    DeliveredCount = table.Column<int>(type: "integer", nullable: false),
                    FailedCount = table.Column<int>(type: "integer", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ParentName = table.Column<string>(type: "text", nullable: true),
                    Mobile = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Standard = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Subject = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Board = table.Column<string>(type: "text", nullable: true),
                    Medium = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    AdminNotes = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AdmissionNumber = table.Column<string>(type: "text", nullable: true),
                    RollNumber = table.Column<string>(type: "text", nullable: true),
                    PhotoUrl = table.Column<string>(type: "text", nullable: true),
                    ParentMobile = table.Column<string>(type: "text", nullable: true),
                    ParentEmail = table.Column<string>(type: "text", nullable: true),
                    EmergencyContact = table.Column<string>(type: "text", nullable: true),
                    PreviousSchool = table.Column<string>(type: "text", nullable: true),
                    DateOfBirth = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudyMaterials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    TitleMarathi = table.Column<string>(type: "text", nullable: false),
                    FileUrl = table.Column<string>(type: "text", nullable: false),
                    Standard = table.Column<string>(type: "text", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    MaterialType = table.Column<string>(type: "text", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UploadedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyMaterials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Mobile = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Qualification = table.Column<string>(type: "text", nullable: true),
                    Specialisation = table.Column<string>(type: "text", nullable: true),
                    PhotoUrl = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    EmployeeCode = table.Column<string>(type: "text", nullable: true),
                    MonthlySalary = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    JoiningDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AdminNotes = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Testimonials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    Quote = table.Column<string>(type: "text", nullable: false),
                    QuoteMarathi = table.Column<string>(type: "text", nullable: true),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Testimonials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentName = table.Column<string>(type: "text", nullable: false),
                    Standard = table.Column<string>(type: "text", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    TotalMarks = table.Column<int>(type: "integer", nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: false),
                    Grade = table.Column<string>(type: "text", nullable: true),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    TestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TestTitle = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Exams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Standard = table.Column<string>(type: "text", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    TotalMarks = table.Column<int>(type: "integer", nullable: false),
                    PassingMarks = table.Column<int>(type: "integer", nullable: false),
                    ExamDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Duration = table.Column<string>(type: "text", nullable: true),
                    BatchId = table.Column<int>(type: "integer", nullable: true),
                    ExamType = table.Column<string>(type: "text", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exams_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FeePayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    FeeType = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Fine = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    PaymentMode = table.Column<string>(type: "text", nullable: false),
                    TransactionRef = table.Column<string>(type: "text", nullable: true),
                    Month = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ReceiptNumber = table.Column<string>(type: "text", nullable: true),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    PaidDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CollectedByUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeePayments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    BatchId = table.Column<int>(type: "integer", nullable: false),
                    JoinDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentBatches_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentBatches_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Attendances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    BatchId = table.Column<int>(type: "integer", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    MarkedByTeacherId = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attendances_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Attendances_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Attendances_Teachers_MarkedByTeacherId",
                        column: x => x.MarkedByTeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BatchSubjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchId = table.Column<int>(type: "integer", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    TeacherId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BatchSubjects_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BatchSubjects_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Homeworks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    Standard = table.Column<string>(type: "text", nullable: false),
                    BatchId = table.Column<int>(type: "integer", nullable: true),
                    AssignedByTeacherId = table.Column<int>(type: "integer", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AttachmentUrl = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Homeworks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Homeworks_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Homeworks_Teachers_AssignedByTeacherId",
                        column: x => x.AssignedByTeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SyllabusItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Standard = table.Column<string>(type: "text", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    ChapterName = table.Column<string>(type: "text", nullable: false),
                    BatchId = table.Column<int>(type: "integer", nullable: true),
                    TeacherId = table.Column<int>(type: "integer", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyllabusItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyllabusItems_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SyllabusItems_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TeacherAttendances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TeacherId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherAttendances_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimetableSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchId = table.Column<int>(type: "integer", nullable: false),
                    TeacherId = table.Column<int>(type: "integer", nullable: true),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Room = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimetableSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimetableSlots_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimetableSlots_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExamResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExamId = table.Column<int>(type: "integer", nullable: false),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    MarksObtained = table.Column<int>(type: "integer", nullable: true),
                    Grade = table.Column<string>(type: "text", nullable: true),
                    Rank = table.Column<int>(type: "integer", nullable: true),
                    IsAbsent = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamResults_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamResults_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HomeworkSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HomeworkId = table.Column<int>(type: "integer", nullable: false),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    FileUrl = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeworkSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HomeworkSubmissions_Homeworks_HomeworkId",
                        column: x => x.HomeworkId,
                        principalTable: "Homeworks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HomeworkSubmissions_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_AppUsers_Email",
                table: "AppUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_Username",
                table: "AppUsers",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_BatchId",
                table: "Attendances",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_MarkedByTeacherId",
                table: "Attendances",
                column: "MarkedByTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_StudentId_BatchId_Date_Subject",
                table: "Attendances",
                columns: new[] { "StudentId", "BatchId", "Date", "Subject" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BatchSubjects_BatchId",
                table: "BatchSubjects",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_BatchSubjects_TeacherId",
                table: "BatchSubjects",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Slug",
                table: "Courses",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_ExamId_StudentId",
                table: "ExamResults",
                columns: new[] { "ExamId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_StudentId",
                table: "ExamResults",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_BatchId",
                table: "Exams",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FeePayments_ReceiptNumber",
                table: "FeePayments",
                column: "ReceiptNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeePayments_StudentId",
                table: "FeePayments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Homeworks_AssignedByTeacherId",
                table: "Homeworks",
                column: "AssignedByTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Homeworks_BatchId",
                table: "Homeworks",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkSubmissions_HomeworkId",
                table: "HomeworkSubmissions",
                column: "HomeworkId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkSubmissions_StudentId",
                table: "HomeworkSubmissions",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentBatches_BatchId",
                table: "StudentBatches",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentBatches_StudentId",
                table: "StudentBatches",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_SyllabusItems_BatchId",
                table: "SyllabusItems",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_SyllabusItems_TeacherId",
                table: "SyllabusItems",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAttendances_TeacherId",
                table: "TeacherAttendances",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableSlots_BatchId",
                table: "TimetableSlots",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableSlots_TeacherId",
                table: "TimetableSlots",
                column: "TeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppUsers");

            migrationBuilder.DropTable(
                name: "Attendances");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "BatchSubjects");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Enquiries");

            migrationBuilder.DropTable(
                name: "ExamResults");

            migrationBuilder.DropTable(
                name: "FeePayments");

            migrationBuilder.DropTable(
                name: "FeeStructures");

            migrationBuilder.DropTable(
                name: "GalleryItems");

            migrationBuilder.DropTable(
                name: "HomePageSectionSettings");

            migrationBuilder.DropTable(
                name: "HomeworkSubmissions");

            migrationBuilder.DropTable(
                name: "Notices");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "StudentBatches");

            migrationBuilder.DropTable(
                name: "StudyMaterials");

            migrationBuilder.DropTable(
                name: "SyllabusItems");

            migrationBuilder.DropTable(
                name: "TeacherAttendances");

            migrationBuilder.DropTable(
                name: "Testimonials");

            migrationBuilder.DropTable(
                name: "TestResults");

            migrationBuilder.DropTable(
                name: "TimetableSlots");

            migrationBuilder.DropTable(
                name: "Exams");

            migrationBuilder.DropTable(
                name: "Homeworks");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Batches");

            migrationBuilder.DropTable(
                name: "Teachers");
        }
    }
}
