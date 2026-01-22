using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyRelocation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyPhotoPrimary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "PropertyPhotos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPhotos_PropertyId_IsPrimary",
                table: "PropertyPhotos",
                columns: new[] { "PropertyId", "IsPrimary" },
                unique: true,
                filter: "\"IsPrimary\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PropertyPhotos_PropertyId_IsPrimary",
                table: "PropertyPhotos");

            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "PropertyPhotos");
        }
    }
}
