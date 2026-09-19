using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shivakala.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMpscStudentModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                table: "OnlineExamAttempts",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "MpscStudentId",
                table: "OnlineExamAttempts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Audience",
                table: "Exams",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "Exams",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Venue",
                table: "Exams",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MpscStudentId",
                table: "AppUsers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MpscStudents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RegistrationNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    FullNameMarathi = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Mobile = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    AlternateMobile = table.Column<string>(type: "TEXT", maxLength: 15, nullable: true),
                    DateOfBirth = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Gender = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PhotoUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    District = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "Maharashtra"),
                    Pincode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    HighestQualification = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DegreeOrCourse = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    University = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    YearOfPassing = table.Column<int>(type: "INTEGER", nullable: true),
                    PreferredExam = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "Rajyaseva"),
                    OtherExamInterest = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    PreparationLevel = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Beginner"),
                    TargetAttemptYear = table.Column<int>(type: "INTEGER", nullable: true),
                    PreviousAttempts = table.Column<bool>(type: "INTEGER", nullable: false),
                    NumberOfPreviousAttempts = table.Column<int>(type: "INTEGER", nullable: true),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Open"),
                    CasteCertificateAvailable = table.Column<bool>(type: "INTEGER", nullable: false),
                    NonCreamyLayerCertificateAvailable = table.Column<bool>(type: "INTEGER", nullable: false),
                    EwsCertificateAvailable = table.Column<bool>(type: "INTEGER", nullable: false),
                    ConsentAccepted = table.Column<bool>(type: "INTEGER", nullable: false),
                    ConsentAcceptedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ConsentIpAddress = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MpscStudents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MpscExamRegistrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExamId = table.Column<int>(type: "INTEGER", nullable: false),
                    MpscStudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Registered"),
                    Mode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Online"),
                    SeatNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    AttendanceStatus = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    AttendanceMarkedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AttendanceRemarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MpscExamRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MpscExamRegistrations_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MpscExamRegistrations_MpscStudents_MpscStudentId",
                        column: x => x.MpscStudentId,
                        principalTable: "MpscStudents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MpscExamResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExamId = table.Column<int>(type: "INTEGER", nullable: false),
                    MpscStudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    MarksObtained = table.Column<double>(type: "REAL", nullable: true),
                    Rank = table.Column<int>(type: "INTEGER", nullable: true),
                    Grade = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    IsAbsent = table.Column<bool>(type: "INTEGER", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Mode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Offline"),
                    OnlineAttemptId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MpscExamResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MpscExamResults_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MpscExamResults_MpscStudents_MpscStudentId",
                        column: x => x.MpscStudentId,
                        principalTable: "MpscStudents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MpscExamResults_OnlineExamAttempts_OnlineAttemptId",
                        column: x => x.OnlineAttemptId,
                        principalTable: "OnlineExamAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OnlineExamAttempts_ExamId_MpscStudentId",
                table: "OnlineExamAttempts",
                columns: new[] { "ExamId", "MpscStudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_OnlineExamAttempts_MpscStudentId",
                table: "OnlineExamAttempts",
                column: "MpscStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_MpscStudentId",
                table: "AppUsers",
                column: "MpscStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_MpscExamRegistrations_ExamId_MpscStudentId",
                table: "MpscExamRegistrations",
                columns: new[] { "ExamId", "MpscStudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MpscExamRegistrations_MpscStudentId",
                table: "MpscExamRegistrations",
                column: "MpscStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_MpscExamResults_ExamId_MpscStudentId",
                table: "MpscExamResults",
                columns: new[] { "ExamId", "MpscStudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_MpscExamResults_MpscStudentId",
                table: "MpscExamResults",
                column: "MpscStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_MpscExamResults_OnlineAttemptId",
                table: "MpscExamResults",
                column: "OnlineAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_MpscStudents_Email",
                table: "MpscStudents",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MpscStudents_Mobile",
                table: "MpscStudents",
                column: "Mobile",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MpscStudents_RegistrationNumber",
                table: "MpscStudents",
                column: "RegistrationNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppUsers_MpscStudents_MpscStudentId",
                table: "AppUsers",
                column: "MpscStudentId",
                principalTable: "MpscStudents",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OnlineExamAttempts_MpscStudents_MpscStudentId",
                table: "OnlineExamAttempts",
                column: "MpscStudentId",
                principalTable: "MpscStudents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppUsers_MpscStudents_MpscStudentId",
                table: "AppUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_OnlineExamAttempts_MpscStudents_MpscStudentId",
                table: "OnlineExamAttempts");

            migrationBuilder.DropTable(
                name: "MpscExamRegistrations");

            migrationBuilder.DropTable(
                name: "MpscExamResults");

            migrationBuilder.DropTable(
                name: "MpscStudents");

            migrationBuilder.DropIndex(
                name: "IX_OnlineExamAttempts_ExamId_MpscStudentId",
                table: "OnlineExamAttempts");

            migrationBuilder.DropIndex(
                name: "IX_OnlineExamAttempts_MpscStudentId",
                table: "OnlineExamAttempts");

            migrationBuilder.DropIndex(
                name: "IX_AppUsers_MpscStudentId",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "MpscStudentId",
                table: "OnlineExamAttempts");

            migrationBuilder.DropColumn(
                name: "Audience",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "Venue",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "MpscStudentId",
                table: "AppUsers");

            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                table: "OnlineExamAttempts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
