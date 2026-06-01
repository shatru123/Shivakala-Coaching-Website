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
            migrationBuilder.AddColumn<string>(
                name: "AdmissionNumber", table: "Students", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "RollNumber", table: "Students", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl", table: "Students", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "ParentMobile", table: "Students", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "ParentEmail", table: "Students", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "EmergencyContact", table: "Students", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "PreviousSchool", table: "Students", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "DateOfBirth", table: "Students", nullable: true);

            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: t => new {
                    Id = t.Column<int>(nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Username = t.Column<string>(maxLength: 100, nullable: false),
                    Email = t.Column<string>(maxLength: 200, nullable: false),
                    PasswordHash = t.Column<string>(maxLength: 200, nullable: false),
                    Role = t.Column<string>(maxLength: 50, nullable: false),
                    FullName = t.Column<string>(nullable: true),
                    Mobile = t.Column<string>(nullable: true),
                    PhotoUrl = t.Column<string>(nullable: true),
                    IsActive = t.Column<bool>(nullable: false, defaultValue: true),
                    PasswordResetToken = t.Column<string>(nullable: true),
                    PasswordResetExpiry = t.Column<DateTime>(nullable: true),
                    CreatedDate = t.Column<DateTime>(nullable: false),
                    LastLoginDate = t.Column<DateTime>(nullable: true),
                    TeacherId = t.Column<int>(nullable: true),
                    StudentId = t.Column<int>(nullable: true)
                },
                constraints: t => t.PrimaryKey("PK_AppUsers", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: t => new {
                    Id = t.Column<int>(nullable: false).Annotation("Sqlite:Autoincrement", true),
                    FullName = t.Column<string>(maxLength: 200, nullable: false),
                    Mobile = t.Column<string>(maxLength: 20, nullable: false),
                    Email = t.Column<string>(nullable: true),
                    Qualification = t.Column<string>(nullable: true),
                    Specialisation = t.Column<string>(nullable: true),
                    PhotoUrl = t.Column<string>(nullable: true),
                    Address = t.Column<string>(nullable: true),
                    EmployeeCode = t.Column<string>(nullable: true),
                    MonthlySalary = t.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    JoiningDate = t.Column<DateTime>(nullable: false),
                    IsActive = t.Column<bool>(nullable: false, defaultValue: true),
                    AdminNotes = t.Column<string>(nullable: true),
                    CreatedDate = t.Column<DateTime>(nullable: false)
                },
                constraints: t => t.PrimaryKey("PK_Teachers", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Batches",
                columns: t => new {
                    Id = t.Column<int>(nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Name = t.Column<string>(maxLength: 200, nullable: false),
                    Standard = t.Column<string>(nullable: false),
                    Medium = t.Column<string>(nullable: true),
                    MaxStrength = t.Column<int>(nullable: false, defaultValue: 30),
                    Room = t.Column<string>(nullable: true),
                    TimingSlot = t.Column<string>(nullable: true),
                    IsActive = t.Column<bool>(nullable: false, defaultValue: true),
                    AcademicYear = t.Column<string>(nullable: false),
                    CreatedDate = t.Column<DateTime>(nullable: false)
                },
                constraints: t => t.PrimaryKey("PK_Batches", x => x.Id));

            migrationBuilder.CreateTable(
                name: "BatchSubjects",
                columns: t => new {
                    Id = t.Column<int>(nullable: false).Annotation("Sqlite:Autoincrement", true),
                    BatchId = t.Column<int>(nullable: false),
                    Subject = t.Column<string>(nullable: false),
                    TeacherId = t.Column<int>(nullable: true)
                },
                constraints: t => {
                    t.PrimaryKey("PK_BatchSubjects", x => x.Id);
                    t.ForeignKey("FK_BatchSubjects_Batches_BatchId", x => x.BatchId, "Batches", "Id", onDelete: ReferentialAction.Cascade);
                    t.ForeignKey("FK_BatchSubjects_Teachers_TeacherId", x => x.TeacherId, "Teachers", "Id", onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "StudentBatches",
                columns: t => new {
                    Id = t.Column<int>(nullable: false).Annotation("Sqlite:Autoincrement", true),
                    StudentId = t.Column<int>(nullable: false),
                    BatchId = t.Column<int>(nullable: false),
                    JoinDate = t.Column<DateTime>(nullable: false),
                    IsActive = t.Column<bool>(nullable: false, defaultValue: true)
                },
                constraints: t => {
                    t.PrimaryKey("PK_StudentBatches", x => x.Id);
                    t.ForeignKey("FK_StudentBatches_Students_StudentId", x => x.StudentId, "Students", "Id", onDelete: ReferentialAction.Restrict);
                    t.ForeignKey("FK_StudentBatches_Batches_BatchId", x => x.BatchId, "Batches", "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Attendances",
                columns: t => new {
                    Id = t.Column<int>(nullable: false).Annotation("Sqlite:Autoincrement", true),
                    StudentId = t.Column<int>(nullable: false),
                    BatchId = t.Column<int>(nullable: false),
                    Subject = t.Column<string>(nullable: true),
                    Date = t.Column<string>(nullable: false),
                    Status = t.Column<string>(nullable: false, defaultValue: "Present"),
                    Remarks = t.Column<string>(nullable: true),
                    MarkedByTeacherId = t.Column<int>(nullable: true),
                    CreatedDate = t.Column<DateTime>(nullable: false)
                },
                constraints: t => {
                    t.PrimaryKey("PK_Attendances", x => x.Id);
                    t.ForeignKey("FK_Attendances_Students_StudentId", x => x.StudentId, "Students", "Id", onDelete: ReferentialAction.Restrict);
                    t.ForeignKey("FK_Attendances_Batches_BatchId", x => x.BatchId, "Batches", "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeacherAttendances",
                columns: t => new {
                    Id = t.Column<int>(nullable: false).Annotation("Sqlite:Autoincrement", true),
                    TeacherId = t.Column<int>(nullable: false),
                    Date = t.Column<string>(nullable: false),
                    Status = t.Column<string>(nullable: false, defaultValue: "Present"),
                    Remarks = t.Column<string>(nullable: true),
                    CreatedDate = t.Column<DateTime>(nullable: false)
                },
                constraints: t => {
                    t.PrimaryKey("PK_TeacherAttendances", x => x.Id);
                    t.ForeignKey("FK_TeacherAttendances_Teachers_TeacherId", x => x.TeacherId, "Teachers", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeeStructures",
                columns: t => new {
                    Id = t.Column<int>(nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Standard = t.Column<string>(nullable: false),
                    FeeType = t.Column<string>(nullable: false),
                    Amount = t.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    AcademicYear = t.Column<string>(nullable: false),
                    IsActive = t.Column<bool>(nullable: false, defaultValue: true),
                    CreatedDate = t.Column<DateTime>(nullable: false)
                },
                constraints: t => t.PrimaryKey("PK_FeeStructures", x => x.Id));

            migrationBuilder.CreateTable(
                name: "FeePayments",
                columns: t => new {
                    Id = t.Column<int>(nullable: false).Annotation("Sqlite:Autoincrement", true),
                    StudentId = t.Column<int>(nullable: false),
                    FeeType = t.Column<string>(nullable: false),
                    Amount = t.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Discount = t.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValue: 0m),
                    Fine = t.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValue: 0m),
                    PaidAmount = t.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PaymentMode = t.Column<string>(nullable: false, defaultValue: "Cash"),
                    TransactionRef = t.Column<string>(nullable: true),
                    Month = t.Column<string>(nullable: false),
                    Status = t.Column<string>(nullable: false, defaultValue: "Paid"),
                    ReceiptNumber = t.Column<string>(nullable: true),
                    Remarks = t.Column<string>(nullable: true),
                    PaidDate = t.Column<DateTime>(nullable: false),
                    CreatedDate = t.Column<DateTime>(nullable: false),
                    CollectedByUserId = t.Column<int>(nullable: true)
                },
                constraints: t => {
                    t.PrimaryKey("PK_FeePayments", x => x.Id);
                    t.ForeignKey("FK_FeePayments_Students_StudentId", x => x.StudentId, "Students", "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Exams",
                columns: t => new {
                    Id = t.Column<int>(nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Title = t.Column<string>(nullable: false),
                    Standard = t.Column<string>(nullable: false),
                    Subject = t.Column<string>(nullable: false),
                    TotalMarks = t.Column<int>(nullable: false, defaultValue: 100),
                    PassingMarks = t.Column<int>(nullable: false, defaultValue: 35),
                    ExamDate = t.Column<DateTime>(nullable: false),
                    Duration = t.Column<string>(nullable: true),
                    BatchId = t.Column<int>(nullable: true),
                    ExamType = t.Column<string>(nullable: false, defaultValue: "Weekly"),
                    IsPublished = t.Column<bool>(nullable: false, defaultValue: false),
                    CreatedDate = t.Column<DateTime>(nullable: false)
                },
                constraints: t => {
                    t.PrimaryKey("PK_Exams", x => x.Id);
                    t.ForeignKey("FK_Exams_Batches_BatchId", x => x.BatchId, "Batches", "Id", onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ExamResults",
                columns: t => new {
                    Id = t.Column<int>(nullable: false).Annotation("Sqlite:Autoincrement", true),
                    ExamId = t.Column<int>(nullable: false),
                    StudentId = t.Column<int>(nullable: false),
                    MarksObtained = t.Column<int>(nullable: true),
                    Grade = t.Column<string>(nullable: true),
                    Rank = t.Column<int>(nullable: true),
                    IsAbsent = t.Column<bool>(nullable: false, defaultValue: false),
                    Remarks = t.Column<string>(nullable: true),
                    CreatedDate = t.Column<DateTime>(nullable: false)
                },
                constraints: t => {
                    t.PrimaryKey("PK_ExamResults", x => x.Id);
                    t.ForeignKey("FK_ExamResults_Exams_ExamId", x => x.ExamId, "Exams", "Id", onDelete: ReferentialAction.Cascade);
                    t.ForeignKey("FK_ExamResults_Students_StudentId", x => x.StudentId, "Students", "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Homeworks",
                columns: t => new {
                    Id = t.Column<int>(nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Title = t.Column<string>(nullable: false),
                    Description = t.Column<string>(nullable: true),
                    Subject = t.Column<string>(nullable: false),
                    Standard = t.Column<string>(nullable: false),
                    BatchId = t.Column<int>(nullable: true),
                    AssignedByTeacherId = t.Column<int>(nullable: false),
                    DueDate = t.Column<DateTime>(nullable: false),
                    AttachmentUrl = t.Column<string>(nullable: true),
                    IsActive = t.Column<bool>(nullable: false, defaultValue: true),
                    CreatedDate = t.Column<DateTime>(nullable: false)
                },
                constraints: t => {
                    t.PrimaryKey("PK_Homeworks", x => x.Id);
                    t.ForeignKey("FK_Homeworks_Teachers_AssignedByTeacherId", x => x.AssignedByTeacherId, "Teachers", "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HomeworkSubmissions",
                columns: t => new {
                    Id = t.Column<int>(nullable: false).Annotation("Sqlite:Autoincrement", true),
                    HomeworkId = t.Column<int>(nullable: false),
                    StudentId = t.Column<int>(nullable: false),
                    FileUrl = t.Column<string>(nullable: true),
                    Notes = t.Column<string>(nullable: true),
                    Status = t.Column<string>(nullable: false, defaultValue: "Submitted"),
                    SubmittedAt = t.Column<DateTime>(nullable: false)
                },
                constraints: t => {
                    t.PrimaryKey("PK_HomeworkSubmissions", x => x.Id);
                    t.ForeignKey("FK_HomeworkSubmissions_Homeworks_HomeworkId", x => x.HomeworkId, "Homeworks", "Id", onDelete: ReferentialAction.Cascade);
                    t.ForeignKey("FK_HomeworkSubmissions_Students_StudentId", x => x.StudentId, "Students", "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TimetableSlots",
                columns: t => new {
                    Id = t.Column<int>(nullable: false).Annotation("Sqlite:Autoincrement", true),
                    BatchId = t.Column<int>(nullable: false),
                    TeacherId = t.Column<int>(nullable: true),
                    Subject = t.Column<string>(nullable: false),
                    DayOfWeek = t.Column<int>(nullable: false),
                    StartTime = t.Column<string>(nullable: false),
                    EndTime = t.Column<string>(nullable: false),
                    Room = t.Column<string>(nullable: true),
                    IsActive = t.Column<bool>(nullable: false, defaultValue: true)
                },
                constraints: t => {
                    t.PrimaryKey("PK_TimetableSlots", x => x.Id);
                    t.ForeignKey("FK_TimetableSlots_Batches_BatchId", x => x.BatchId, "Batches", "Id", onDelete: ReferentialAction.Cascade);
                    t.ForeignKey("FK_TimetableSlots_Teachers_TeacherId", x => x.TeacherId, "Teachers", "Id", onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: t => new {
                    Id = t.Column<int>(nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Title = t.Column<string>(nullable: false),
                    Message = t.Column<string>(nullable: false),
                    Channel = t.Column<string>(nullable: false),
                    Audience = t.Column<string>(nullable: false),
                    Status = t.Column<string>(nullable: false, defaultValue: "Pending"),
                    TemplateKey = t.Column<string>(nullable: true),
                    AttachmentUrl = t.Column<string>(nullable: true),
                    SentByUserId = t.Column<int>(nullable: true),
                    DeliveredCount = t.Column<int>(nullable: false, defaultValue: 0),
                    FailedCount = t.Column<int>(nullable: false, defaultValue: 0),
                    SentAt = t.Column<DateTime>(nullable: true),
                    CreatedDate = t.Column<DateTime>(nullable: false)
                },
                constraints: t => t.PrimaryKey("PK_Notifications", x => x.Id));

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: t => new {
                    Id = t.Column<int>(nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Action = t.Column<string>(nullable: false),
                    EntityType = t.Column<string>(nullable: false),
                    EntityId = t.Column<int>(nullable: true),
                    OldValues = t.Column<string>(nullable: true),
                    NewValues = t.Column<string>(nullable: true),
                    PerformedByUsername = t.Column<string>(nullable: true),
                    IpAddress = t.Column<string>(nullable: true),
                    CreatedDate = t.Column<DateTime>(nullable: false)
                },
                constraints: t => t.PrimaryKey("PK_AuditLogs", x => x.Id));

            migrationBuilder.CreateTable(
                name: "SyllabusItems",
                columns: t => new {
                    Id = t.Column<int>(nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Standard = t.Column<string>(nullable: false),
                    Subject = t.Column<string>(nullable: false),
                    ChapterName = t.Column<string>(nullable: false),
                    BatchId = t.Column<int>(nullable: true),
                    TeacherId = t.Column<int>(nullable: true),
                    DisplayOrder = t.Column<int>(nullable: false, defaultValue: 0),
                    IsCompleted = t.Column<bool>(nullable: false, defaultValue: false),
                    CompletedDate = t.Column<DateTime>(nullable: true),
                    AttachmentUrl = t.Column<string>(nullable: true),
                    Notes = t.Column<string>(nullable: true),
                    CreatedDate = t.Column<DateTime>(nullable: false)
                },
                constraints: t => t.PrimaryKey("PK_SyllabusItems", x => x.Id));

            // Indexes
            migrationBuilder.CreateIndex("IX_AppUsers_Username", "AppUsers", "Username", unique: true);
            migrationBuilder.CreateIndex("IX_AppUsers_Email", "AppUsers", "Email", unique: true);
            migrationBuilder.CreateIndex("IX_FeePayments_StudentId", "FeePayments", "StudentId");
            migrationBuilder.CreateIndex("IX_FeePayments_ReceiptNumber", "FeePayments", "ReceiptNumber", unique: true, filter: "[ReceiptNumber] IS NOT NULL");
            migrationBuilder.CreateIndex("IX_Attendances_StudentBatchDate", "Attendances", new[] { "StudentId", "BatchId", "Date" });
            migrationBuilder.CreateIndex("IX_ExamResults_ExamStudent", "ExamResults", new[] { "ExamId", "StudentId" }, unique: true);
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
        }
    }
}
