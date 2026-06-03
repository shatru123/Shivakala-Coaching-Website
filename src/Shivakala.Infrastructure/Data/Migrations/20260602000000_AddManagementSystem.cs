using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shivakala.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddManagementSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Extend Students table ─────────────────────────────────────────
            migrationBuilder.AddColumn<string>(
                name: "AdmissionNumber", table: "Students", type: "TEXT", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "RollNumber", table: "Students", type: "TEXT", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl", table: "Students", type: "TEXT", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "ParentMobile", table: "Students", type: "TEXT", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "ParentEmail", table: "Students", type: "TEXT", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "EmergencyContact", table: "Students", type: "TEXT", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "PreviousSchool", table: "Students", type: "TEXT", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "DateOfBirth", table: "Students", type: "TEXT", nullable: true);

            // ── AppUsers ──────────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: t => new {
                    Id                  = t.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Username            = t.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email               = t.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    PasswordHash        = t.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Role                = t.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FullName            = t.Column<string>(type: "TEXT", nullable: true),
                    Mobile              = t.Column<string>(type: "TEXT", nullable: true),
                    PhotoUrl            = t.Column<string>(type: "TEXT", nullable: true),
                    IsActive            = t.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    PasswordResetToken  = t.Column<string>(type: "TEXT", nullable: true),
                    PasswordResetExpiry = t.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedDate         = t.Column<DateTime>(type: "TEXT", nullable: false),
                    LastLoginDate       = t.Column<DateTime>(type: "TEXT", nullable: true),
                    TeacherId           = t.Column<int>(type: "INTEGER", nullable: true),
                    StudentId           = t.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: t => t.PrimaryKey("PK_AppUsers", x => x.Id));

            // ── Teachers ──────────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: t => new {
                    Id            = t.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    FullName      = t.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Mobile        = t.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Email         = t.Column<string>(type: "TEXT", nullable: true),
                    Qualification = t.Column<string>(type: "TEXT", nullable: true),
                    Specialisation= t.Column<string>(type: "TEXT", nullable: true),
                    PhotoUrl      = t.Column<string>(type: "TEXT", nullable: true),
                    Address       = t.Column<string>(type: "TEXT", nullable: true),
                    EmployeeCode  = t.Column<string>(type: "TEXT", nullable: true),
                    MonthlySalary = t.Column<decimal>(type: "TEXT", nullable: true),
                    JoiningDate   = t.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive      = t.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    AdminNotes    = t.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate   = t.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: t => t.PrimaryKey("PK_Teachers", x => x.Id));

            // ── Batches ───────────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Batches",
                columns: t => new {
                    Id           = t.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Name         = t.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Standard     = t.Column<string>(type: "TEXT", nullable: false),
                    Medium       = t.Column<string>(type: "TEXT", nullable: true),
                    MaxStrength  = t.Column<int>(type: "INTEGER", nullable: false, defaultValue: 30),
                    Room         = t.Column<string>(type: "TEXT", nullable: true),
                    TimingSlot   = t.Column<string>(type: "TEXT", nullable: true),
                    IsActive     = t.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    AcademicYear = t.Column<string>(type: "TEXT", nullable: false),
                    CreatedDate  = t.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: t => t.PrimaryKey("PK_Batches", x => x.Id));

            // ── BatchSubjects ─────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "BatchSubjects",
                columns: t => new {
                    Id        = t.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    BatchId   = t.Column<int>(type: "INTEGER", nullable: false),
                    Subject   = t.Column<string>(type: "TEXT", nullable: false),
                    TeacherId = t.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: t => {
                    t.PrimaryKey("PK_BatchSubjects", x => x.Id);
                    t.ForeignKey("FK_BatchSubjects_Batches_BatchId", x => x.BatchId, "Batches", "Id",
                        onDelete: ReferentialAction.Cascade);
                    t.ForeignKey("FK_BatchSubjects_Teachers_TeacherId", x => x.TeacherId, "Teachers", "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            // ── StudentBatches ────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "StudentBatches",
                columns: t => new {
                    Id        = t.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    StudentId = t.Column<int>(type: "INTEGER", nullable: false),
                    BatchId   = t.Column<int>(type: "INTEGER", nullable: false),
                    JoinDate  = t.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive  = t.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: t => {
                    t.PrimaryKey("PK_StudentBatches", x => x.Id);
                    t.ForeignKey("FK_StudentBatches_Students_StudentId", x => x.StudentId, "Students", "Id",
                        onDelete: ReferentialAction.Restrict);
                    t.ForeignKey("FK_StudentBatches_Batches_BatchId", x => x.BatchId, "Batches", "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // ── Attendances ───────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Attendances",
                columns: t => new {
                    Id                = t.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    StudentId         = t.Column<int>(type: "INTEGER", nullable: false),
                    BatchId           = t.Column<int>(type: "INTEGER", nullable: false),
                    Subject           = t.Column<string>(type: "TEXT", nullable: true),
                    Date              = t.Column<string>(type: "TEXT", nullable: false),
                    Status            = t.Column<string>(type: "TEXT", nullable: false, defaultValue: "Present"),
                    Remarks           = t.Column<string>(type: "TEXT", nullable: true),
                    MarkedByTeacherId = t.Column<int>(type: "INTEGER", nullable: true),
                    CreatedDate       = t.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: t => {
                    t.PrimaryKey("PK_Attendances", x => x.Id);
                    t.ForeignKey("FK_Attendances_Students_StudentId", x => x.StudentId, "Students", "Id",
                        onDelete: ReferentialAction.Restrict);
                    t.ForeignKey("FK_Attendances_Batches_BatchId", x => x.BatchId, "Batches", "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // ── TeacherAttendances ────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "TeacherAttendances",
                columns: t => new {
                    Id          = t.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    TeacherId   = t.Column<int>(type: "INTEGER", nullable: false),
                    Date        = t.Column<string>(type: "TEXT", nullable: false),
                    Status      = t.Column<string>(type: "TEXT", nullable: false, defaultValue: "Present"),
                    Remarks     = t.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = t.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: t => {
                    t.PrimaryKey("PK_TeacherAttendances", x => x.Id);
                    t.ForeignKey("FK_TeacherAttendances_Teachers_TeacherId", x => x.TeacherId, "Teachers", "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // ── FeeStructures ─────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "FeeStructures",
                columns: t => new {
                    Id           = t.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Standard     = t.Column<string>(type: "TEXT", nullable: false),
                    FeeType      = t.Column<string>(type: "TEXT", nullable: false),
                    Amount       = t.Column<decimal>(type: "TEXT", nullable: false),
                    AcademicYear = t.Column<string>(type: "TEXT", nullable: false),
                    IsActive     = t.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedDate  = t.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: t => t.PrimaryKey("PK_FeeStructures", x => x.Id));

            // ── FeePayments ───────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "FeePayments",
                columns: t => new {
                    Id                = t.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    StudentId         = t.Column<int>(type: "INTEGER", nullable: false),
                    FeeType           = t.Column<string>(type: "TEXT", nullable: false),
                    Amount            = t.Column<decimal>(type: "TEXT", nullable: false),
                    Discount          = t.Column<decimal>(type: "TEXT", nullable: false, defaultValue: 0m),
                    Fine              = t.Column<decimal>(type: "TEXT", nullable: false, defaultValue: 0m),
                    PaidAmount        = t.Column<decimal>(type: "TEXT", nullable: false),
                    PaymentMode       = t.Column<string>(type: "TEXT", nullable: false, defaultValue: "Cash"),
                    TransactionRef    = t.Column<string>(type: "TEXT", nullable: true),
                    Month             = t.Column<string>(type: "TEXT", nullable: false),
                    Status            = t.Column<string>(type: "TEXT", nullable: false, defaultValue: "Paid"),
                    ReceiptNumber     = t.Column<string>(type: "TEXT", nullable: true),
                    Remarks           = t.Column<string>(type: "TEXT", nullable: true),
                    PaidDate          = t.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedDate       = t.Column<DateTime>(type: "TEXT", nullable: false),
                    CollectedByUserId = t.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: t => {
                    t.PrimaryKey("PK_FeePayments", x => x.Id);
                    t.ForeignKey("FK_FeePayments_Students_StudentId", x => x.StudentId, "Students", "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // ── Exams ─────────────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Exams",
                columns: t => new {
                    Id           = t.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Title        = t.Column<string>(type: "TEXT", nullable: false),
                    Standard     = t.Column<string>(type: "TEXT", nullable: false),
                    Subject      = t.Column<string>(type: "TEXT", nullable: false),
                    TotalMarks   = t.Column<int>(type: "INTEGER", nullable: false, defaultValue: 100),
                    PassingMarks = t.Column<int>(type: "INTEGER", nullable: false, defaultValue: 35),
                    ExamDate     = t.Column<DateTime>(type: "TEXT", nullable: false),
                    Duration     = t.Column<string>(type: "TEXT", nullable: true),
                    BatchId      = t.Column<int>(type: "INTEGER", nullable: true),
                    ExamType     = t.Column<string>(type: "TEXT", nullable: false, defaultValue: "Weekly"),
                    IsPublished  = t.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    CreatedDate  = t.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: t => {
                    t.PrimaryKey("PK_Exams", x => x.Id);
                    t.ForeignKey("FK_Exams_Batches_BatchId", x => x.BatchId, "Batches", "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            // ── ExamResults ───────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "ExamResults",
                columns: t => new {
                    Id            = t.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    ExamId        = t.Column<int>(type: "INTEGER", nullable: false),
                    StudentId     = t.Column<int>(type: "INTEGER", nullable: false),
                    MarksObtained = t.Column<int>(type: "INTEGER", nullable: true),
                    Grade         = t.Column<string>(type: "TEXT", nullable: true),
                    Rank          = t.Column<int>(type: "INTEGER", nullable: true),
                    IsAbsent      = t.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    Remarks       = t.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate   = t.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: t => {
                    t.PrimaryKey("PK_ExamResults", x => x.Id);
                    t.ForeignKey("FK_ExamResults_Exams_ExamId", x => x.ExamId, "Exams", "Id",
                        onDelete: ReferentialAction.Cascade);
                    t.ForeignKey("FK_ExamResults_Students_StudentId", x => x.StudentId, "Students", "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // ── Homeworks ─────────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Homeworks",
                columns: t => new {
                    Id                  = t.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Title               = t.Column<string>(type: "TEXT", nullable: false),
                    Description         = t.Column<string>(type: "TEXT", nullable: true),
                    Subject             = t.Column<string>(type: "TEXT", nullable: false),
                    Standard            = t.Column<string>(type: "TEXT", nullable: false),
                    BatchId             = t.Column<int>(type: "INTEGER", nullable: true),
                    AssignedByTeacherId = t.Column<int>(type: "INTEGER", nullable: false),
                    DueDate             = t.Column<DateTime>(type: "TEXT", nullable: false),
                    AttachmentUrl       = t.Column<string>(type: "TEXT", nullable: true),
                    IsActive            = t.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedDate         = t.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: t => {
                    t.PrimaryKey("PK_Homeworks", x => x.Id);
                    t.ForeignKey("FK_Homeworks_Teachers_AssignedByTeacherId", x => x.AssignedByTeacherId,
                        "Teachers", "Id", onDelete: ReferentialAction.Restrict);
                });

            // ── HomeworkSubmissions ───────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "HomeworkSubmissions",
                columns: t => new {
                    Id          = t.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    HomeworkId  = t.Column<int>(type: "INTEGER", nullable: false),
                    StudentId   = t.Column<int>(type: "INTEGER", nullable: false),
                    FileUrl     = t.Column<string>(type: "TEXT", nullable: true),
                    Notes       = t.Column<string>(type: "TEXT", nullable: true),
                    Status      = t.Column<string>(type: "TEXT", nullable: false, defaultValue: "Submitted"),
                    SubmittedAt = t.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: t => {
                    t.PrimaryKey("PK_HomeworkSubmissions", x => x.Id);
                    t.ForeignKey("FK_HomeworkSubmissions_Homeworks_HomeworkId", x => x.HomeworkId,
                        "Homeworks", "Id", onDelete: ReferentialAction.Cascade);
                    t.ForeignKey("FK_HomeworkSubmissions_Students_StudentId", x => x.StudentId,
                        "Students", "Id", onDelete: ReferentialAction.Restrict);
                });

            // ── TimetableSlots ────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "TimetableSlots",
                columns: t => new {
                    Id        = t.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    BatchId   = t.Column<int>(type: "INTEGER", nullable: false),
                    TeacherId = t.Column<int>(type: "INTEGER", nullable: true),
                    Subject   = t.Column<string>(type: "TEXT", nullable: false),
                    DayOfWeek = t.Column<int>(type: "INTEGER", nullable: false),
                    StartTime = t.Column<string>(type: "TEXT", nullable: false),
                    EndTime   = t.Column<string>(type: "TEXT", nullable: false),
                    Room      = t.Column<string>(type: "TEXT", nullable: true),
                    IsActive  = t.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: t => {
                    t.PrimaryKey("PK_TimetableSlots", x => x.Id);
                    t.ForeignKey("FK_TimetableSlots_Batches_BatchId", x => x.BatchId,
                        "Batches", "Id", onDelete: ReferentialAction.Cascade);
                    t.ForeignKey("FK_TimetableSlots_Teachers_TeacherId", x => x.TeacherId,
                        "Teachers", "Id", onDelete: ReferentialAction.SetNull);
                });

            // ── Notifications ─────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: t => new {
                    Id             = t.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Title          = t.Column<string>(type: "TEXT", nullable: false),
                    Message        = t.Column<string>(type: "TEXT", nullable: false),
                    Channel        = t.Column<string>(type: "TEXT", nullable: false),
                    Audience       = t.Column<string>(type: "TEXT", nullable: false),
                    Status         = t.Column<string>(type: "TEXT", nullable: false, defaultValue: "Pending"),
                    TemplateKey    = t.Column<string>(type: "TEXT", nullable: true),
                    AttachmentUrl  = t.Column<string>(type: "TEXT", nullable: true),
                    SentByUserId   = t.Column<int>(type: "INTEGER", nullable: true),
                    DeliveredCount = t.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    FailedCount    = t.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    SentAt         = t.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedDate    = t.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: t => t.PrimaryKey("PK_Notifications", x => x.Id));

            // ── AuditLogs ─────────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: t => new {
                    Id                  = t.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Action              = t.Column<string>(type: "TEXT", nullable: false),
                    EntityType          = t.Column<string>(type: "TEXT", nullable: false),
                    EntityId            = t.Column<int>(type: "INTEGER", nullable: true),
                    OldValues           = t.Column<string>(type: "TEXT", nullable: true),
                    NewValues           = t.Column<string>(type: "TEXT", nullable: true),
                    PerformedByUsername = t.Column<string>(type: "TEXT", nullable: true),
                    IpAddress           = t.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate         = t.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: t => t.PrimaryKey("PK_AuditLogs", x => x.Id));

            // ── SyllabusItems ─────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "SyllabusItems",
                columns: t => new {
                    Id            = t.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Standard      = t.Column<string>(type: "TEXT", nullable: false),
                    Subject       = t.Column<string>(type: "TEXT", nullable: false),
                    ChapterName   = t.Column<string>(type: "TEXT", nullable: false),
                    BatchId       = t.Column<int>(type: "INTEGER", nullable: true),
                    TeacherId     = t.Column<int>(type: "INTEGER", nullable: true),
                    DisplayOrder  = t.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    IsCompleted   = t.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    CompletedDate = t.Column<DateTime>(type: "TEXT", nullable: true),
                    AttachmentUrl = t.Column<string>(type: "TEXT", nullable: true),
                    Notes         = t.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate   = t.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: t => t.PrimaryKey("PK_SyllabusItems", x => x.Id));

            // ── Indexes ───────────────────────────────────────────────────────
            migrationBuilder.CreateIndex("IX_AppUsers_Username", "AppUsers", "Username", unique: true);
            migrationBuilder.CreateIndex("IX_AppUsers_Email",    "AppUsers", "Email",    unique: true);
            migrationBuilder.CreateIndex("IX_FeePayments_StudentId", "FeePayments", "StudentId");
            migrationBuilder.CreateIndex(
                name:   "IX_FeePayments_ReceiptNumber",
                table:  "FeePayments",
                column: "ReceiptNumber",
                unique: true,
                filter: "\"ReceiptNumber\" IS NOT NULL");
            migrationBuilder.CreateIndex(
                name:    "IX_Attendances_StudentBatchDate",
                table:   "Attendances",
                columns: new[] { "StudentId", "BatchId", "Date" });
            migrationBuilder.CreateIndex(
                name:    "IX_ExamResults_ExamStudent",
                table:   "ExamResults",
                columns: new[] { "ExamId", "StudentId" },
                unique:  true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("SyllabusItems");
            migrationBuilder.DropTable("AuditLogs");
            migrationBuilder.DropTable("Notifications");
            migrationBuilder.DropTable("TimetableSlots");
            migrationBuilder.DropTable("HomeworkSubmissions");
            migrationBuilder.DropTable("Homeworks");
            migrationBuilder.DropTable("ExamResults");
            migrationBuilder.DropTable("Exams");
            migrationBuilder.DropTable("FeePayments");
            migrationBuilder.DropTable("FeeStructures");
            migrationBuilder.DropTable("TeacherAttendances");
            migrationBuilder.DropTable("Attendances");
            migrationBuilder.DropTable("StudentBatches");
            migrationBuilder.DropTable("BatchSubjects");
            migrationBuilder.DropTable("Batches");
            migrationBuilder.DropTable("Teachers");
            migrationBuilder.DropTable("AppUsers");
            migrationBuilder.DropColumn("AdmissionNumber",  "Students");
            migrationBuilder.DropColumn("RollNumber",       "Students");
            migrationBuilder.DropColumn("PhotoUrl",         "Students");
            migrationBuilder.DropColumn("ParentMobile",     "Students");
            migrationBuilder.DropColumn("ParentEmail",      "Students");
            migrationBuilder.DropColumn("EmergencyContact", "Students");
            migrationBuilder.DropColumn("PreviousSchool",   "Students");
            migrationBuilder.DropColumn("DateOfBirth",      "Students");
        }
    }
}
