using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jobportalwebsite.Migrations
{
    /// <inheritdoc />
    public partial class UpdateJobSkillTestAndAnswers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "TestResults");

            migrationBuilder.DropTable(
                name: "SkillTests");

            migrationBuilder.AddColumn<string>(
                name: "PerformanceBadge",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TestScore",
                table: "Applications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "JobSkillTests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OptionA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OptionB = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OptionC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OptionD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrectAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSkillTests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobSkillTests_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobSeekerAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    JobSkillTestId = table.Column<int>(type: "int", nullable: false),
                    SelectedAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSeekerAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobSeekerAnswers_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JobSeekerAnswers_JobSkillTests_JobSkillTestId",
                        column: x => x.JobSkillTestId,
                        principalTable: "JobSkillTests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerAnswers_ApplicationId",
                table: "JobSeekerAnswers",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerAnswers_JobSkillTestId",
                table: "JobSeekerAnswers",
                column: "JobSkillTestId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSkillTests_JobId",
                table: "JobSkillTests",
                column: "JobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobSeekerAnswers");

            migrationBuilder.DropTable(
                name: "JobSkillTests");

            migrationBuilder.DropColumn(
                name: "PerformanceBadge",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "TestScore",
                table: "Applications");

            migrationBuilder.CreateTable(
                name: "SkillTests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Difficulty = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillTests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SkillTestId = table.Column<int>(type: "int", nullable: false),
                    CorrectAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OptionA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OptionB = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OptionC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OptionD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_SkillTests_SkillTestId",
                        column: x => x.SkillTestId,
                        principalTable: "SkillTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SkillTestId = table.Column<int>(type: "int", nullable: false),
                    BadgeEarned = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletionTime = table.Column<int>(type: "int", nullable: true),
                    Score = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestResults_SkillTests_SkillTestId",
                        column: x => x.SkillTestId,
                        principalTable: "SkillTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_SkillTestId",
                table: "Questions",
                column: "SkillTestId");

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_SkillTestId",
                table: "TestResults",
                column: "SkillTestId");
        }
    }
}
