using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shivakala.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOnlineExamEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowMultipleAttempts",
                table: "Exams",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Exams",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "Exams",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDateTime",
                table: "Exams",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExamMode",
                table: "Exams",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "Exams",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRegistrationRequired",
                table: "Exams",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaximumAttempts",
                table: "Exams",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "NegativeMarkingEnabled",
                table: "Exams",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "NegativeMarks",
                table: "Exams",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "QuestionCount",
                table: "Exams",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "RandomizeOptions",
                table: "Exams",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RandomizeQuestions",
                table: "Exams",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationCloseDate",
                table: "Exams",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationOpenDate",
                table: "Exams",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultReleaseMode",
                table: "Exams",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ShowCorrectAnswers",
                table: "Exams",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDateTime",
                table: "Exams",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExamRegistrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExamId = table.Column<int>(type: "INTEGER", nullable: false),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    AttemptCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamRegistrations_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamRegistrations_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OnlineExamAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExamId = table.Column<int>(type: "INTEGER", nullable: false),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    AttemptNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    Score = table.Column<double>(type: "REAL", nullable: false),
                    TotalMarks = table.Column<int>(type: "INTEGER", nullable: false),
                    CorrectCount = table.Column<int>(type: "INTEGER", nullable: false),
                    IncorrectCount = table.Column<int>(type: "INTEGER", nullable: false),
                    UnansweredCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Percentage = table.Column<double>(type: "REAL", nullable: false),
                    TimeTakenSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    IsAutoSubmitted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlineExamAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnlineExamAttempts_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OnlineExamAttempts_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QuestionText = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    QuestionType = table.Column<string>(type: "TEXT", nullable: false),
                    Marks = table.Column<int>(type: "INTEGER", nullable: false),
                    NegativeMarks = table.Column<double>(type: "REAL", nullable: false),
                    Explanation = table.Column<string>(type: "TEXT", nullable: true),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Standard = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Topic = table.Column<string>(type: "TEXT", nullable: true),
                    Difficulty = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Medium"),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExamQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExamId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuestionId = table.Column<int>(type: "INTEGER", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    MarksOverride = table.Column<int>(type: "INTEGER", nullable: true),
                    NegativeMarksOverride = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamQuestions_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamQuestions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuestionOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QuestionId = table.Column<int>(type: "INTEGER", nullable: false),
                    OptionText = table.Column<string>(type: "TEXT", nullable: false),
                    OptionImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    OptionKey = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    IsCorrect = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionOptions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OnlineExamAttemptAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AttemptId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuestionId = table.Column<int>(type: "INTEGER", nullable: false),
                    SelectedOptionId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsMarkedForReview = table.Column<bool>(type: "INTEGER", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlineExamAttemptAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnlineExamAttemptAnswers_OnlineExamAttempts_AttemptId",
                        column: x => x.AttemptId,
                        principalTable: "OnlineExamAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OnlineExamAttemptAnswers_QuestionOptions_SelectedOptionId",
                        column: x => x.SelectedOptionId,
                        principalTable: "QuestionOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OnlineExamAttemptAnswers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamQuestions_ExamId_QuestionId",
                table: "ExamQuestions",
                columns: new[] { "ExamId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamQuestions_QuestionId",
                table: "ExamQuestions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrations_ExamId_StudentId",
                table: "ExamRegistrations",
                columns: new[] { "ExamId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrations_StudentId",
                table: "ExamRegistrations",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineExamAttemptAnswers_AttemptId_QuestionId",
                table: "OnlineExamAttemptAnswers",
                columns: new[] { "AttemptId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OnlineExamAttemptAnswers_QuestionId",
                table: "OnlineExamAttemptAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineExamAttemptAnswers_SelectedOptionId",
                table: "OnlineExamAttemptAnswers",
                column: "SelectedOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineExamAttempts_ExamId_StudentId",
                table: "OnlineExamAttempts",
                columns: new[] { "ExamId", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_OnlineExamAttempts_StudentId",
                table: "OnlineExamAttempts",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptions_QuestionId",
                table: "QuestionOptions",
                column: "QuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamQuestions");

            migrationBuilder.DropTable(
                name: "ExamRegistrations");

            migrationBuilder.DropTable(
                name: "OnlineExamAttemptAnswers");

            migrationBuilder.DropTable(
                name: "OnlineExamAttempts");

            migrationBuilder.DropTable(
                name: "QuestionOptions");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropColumn(
                name: "AllowMultipleAttempts",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "EndDateTime",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "ExamMode",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "IsRegistrationRequired",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "MaximumAttempts",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "NegativeMarkingEnabled",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "NegativeMarks",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "QuestionCount",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "RandomizeOptions",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "RandomizeQuestions",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "RegistrationCloseDate",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "RegistrationOpenDate",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "ResultReleaseMode",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "ShowCorrectAnswers",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "StartDateTime",
                table: "Exams");
        }
    }
}
