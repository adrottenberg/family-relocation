using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyRelocation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAgreementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BrokerAgreementDocumentUrl",
                table: "HousingSearches",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BrokerAgreementSigned",
                table: "HousingSearches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "BrokerAgreementSignedDate",
                table: "HousingSearches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommunityTakanosDocumentUrl",
                table: "HousingSearches",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CommunityTakanosSigned",
                table: "HousingSearches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CommunityTakanosSignedDate",
                table: "HousingSearches",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrokerAgreementDocumentUrl",
                table: "HousingSearches");

            migrationBuilder.DropColumn(
                name: "BrokerAgreementSigned",
                table: "HousingSearches");

            migrationBuilder.DropColumn(
                name: "BrokerAgreementSignedDate",
                table: "HousingSearches");

            migrationBuilder.DropColumn(
                name: "CommunityTakanosDocumentUrl",
                table: "HousingSearches");

            migrationBuilder.DropColumn(
                name: "CommunityTakanosSigned",
                table: "HousingSearches");

            migrationBuilder.DropColumn(
                name: "CommunityTakanosSignedDate",
                table: "HousingSearches");
        }
    }
}
