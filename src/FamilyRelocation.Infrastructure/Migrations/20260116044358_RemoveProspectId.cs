using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyRelocation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProspectId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applicants_BoardDecision",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Budget",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Budget_Currency",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "DownPayment",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "DownPayment_Currency",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "FatherName",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "HousingNotes",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "LoanTermYears",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "MinBathrooms",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "MinBedrooms",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "MortgageInterestRate",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "MoveTimeline",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "PhoneNumbers",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "ProspectId",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "WifeFatherName",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "WifeFirstName",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "WifeHighSchool",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "WifeMaidenName",
                table: "Applicants");

            migrationBuilder.RenameColumn(
                name: "ShulProximity",
                table: "Applicants",
                newName: "Wife");

            migrationBuilder.RenameColumn(
                name: "RequiredFeatures",
                table: "Applicants",
                newName: "Husband");

            migrationBuilder.CreateTable(
                name: "HousingSearches",
                columns: table => new
                {
                    HousingSearchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Stage = table.Column<int>(type: "integer", nullable: false),
                    StageChangedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrentContract = table.Column<string>(type: "jsonb", nullable: true),
                    FailedContracts = table.Column<string>(type: "jsonb", nullable: false),
                    MovedInStatus = table.Column<int>(type: "integer", nullable: true),
                    MovedInDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Preferences = table.Column<string>(type: "jsonb", nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HousingSearches", x => x.HousingSearchId);
                    table.ForeignKey(
                        name: "FK_HousingSearches_Applicants_ApplicantId",
                        column: x => x.ApplicantId,
                        principalTable: "Applicants",
                        principalColumn: "ApplicantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HousingSearches_ApplicantId",
                table: "HousingSearches",
                column: "ApplicantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HousingSearches_CreatedDate",
                table: "HousingSearches",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_HousingSearches_IsActive",
                table: "HousingSearches",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_HousingSearches_Stage",
                table: "HousingSearches",
                column: "Stage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HousingSearches");

            migrationBuilder.RenameColumn(
                name: "Wife",
                table: "Applicants",
                newName: "ShulProximity");

            migrationBuilder.RenameColumn(
                name: "Husband",
                table: "Applicants",
                newName: "RequiredFeatures");

            migrationBuilder.AddColumn<decimal>(
                name: "Budget",
                table: "Applicants",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Budget_Currency",
                table: "Applicants",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true,
                defaultValue: "USD");

            migrationBuilder.AddColumn<decimal>(
                name: "DownPayment",
                table: "Applicants",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DownPayment_Currency",
                table: "Applicants",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true,
                defaultValue: "USD");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Applicants",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FatherName",
                table: "Applicants",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Applicants",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HousingNotes",
                table: "Applicants",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Applicants",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LoanTermYears",
                table: "Applicants",
                type: "integer",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.AddColumn<decimal>(
                name: "MinBathrooms",
                table: "Applicants",
                type: "numeric(3,1)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinBedrooms",
                table: "Applicants",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MortgageInterestRate",
                table: "Applicants",
                type: "numeric(5,3)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MoveTimeline",
                table: "Applicants",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumbers",
                table: "Applicants",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ProspectId",
                table: "Applicants",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WifeFatherName",
                table: "Applicants",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WifeFirstName",
                table: "Applicants",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WifeHighSchool",
                table: "Applicants",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WifeMaidenName",
                table: "Applicants",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActualClosingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApplicationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ClosingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContractDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContractPropertyId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ModifiedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MovedInDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MovedInStatus = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Stage = table.Column<int>(type: "integer", nullable: false),
                    StageChangedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ContractPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ContractPrice_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true, defaultValue: "USD")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.ApplicationId);
                    table.ForeignKey(
                        name: "FK_Applications_Applicants_ApplicantId",
                        column: x => x.ApplicantId,
                        principalTable: "Applicants",
                        principalColumn: "ApplicantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_BoardDecision",
                table: "Applicants",
                column: "BoardDecision");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ApplicantId",
                table: "Applications",
                column: "ApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ApplicationNumber",
                table: "Applications",
                column: "ApplicationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ContractPropertyId",
                table: "Applications",
                column: "ContractPropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_CreatedDate",
                table: "Applications",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_IsActive",
                table: "Applications",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_Stage",
                table: "Applications",
                column: "Stage");
        }
    }
}
