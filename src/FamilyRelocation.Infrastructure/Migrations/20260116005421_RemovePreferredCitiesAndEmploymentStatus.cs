using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyRelocation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovePreferredCitiesAndEmploymentStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmploymentStatus",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "PreferredCities",
                table: "Applicants");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmploymentStatus",
                table: "Applicants",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredCities",
                table: "Applicants",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }
    }
}
