using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyRelocation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAgreementBoolColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrokerAgreementSigned",
                table: "HousingSearches");

            migrationBuilder.DropColumn(
                name: "CommunityTakanosSigned",
                table: "HousingSearches");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BrokerAgreementSigned",
                table: "HousingSearches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CommunityTakanosSigned",
                table: "HousingSearches",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
