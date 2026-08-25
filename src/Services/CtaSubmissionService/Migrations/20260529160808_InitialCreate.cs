using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CtaSubmissionService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cta_submissions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    FormName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CtaType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Language = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    SourcePath = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    ValuesJson = table.Column<string>(type: "TEXT", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: true),
                    LastName = table.Column<string>(type: "TEXT", nullable: true),
                    Gender = table.Column<string>(type: "TEXT", nullable: true),
                    ReasonForJoining = table.Column<string>(type: "TEXT", nullable: true),
                    PresentOrganization = table.Column<string>(type: "TEXT", nullable: true),
                    VolunteeingExperience = table.Column<string>(type: "TEXT", nullable: true),
                    DateOfBirth = table.Column<string>(type: "TEXT", nullable: true),
                    CityOfResidence = table.Column<string>(type: "TEXT", nullable: true),
                    CountryOfResidence = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cta_submissions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cta_submissions_CreatedAt",
                table: "cta_submissions",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cta_submissions");
        }
    }
}
