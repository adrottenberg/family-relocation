using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyRelocation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicantAndApplicationEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Applicants",
                columns: table => new
                {
                    ApplicantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProspectId = table.Column<Guid>(type: "uuid", nullable: true),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FatherName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WifeFirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WifeMaidenName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WifeFatherName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WifeHighSchool = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PhoneNumbers = table.Column<string>(type: "jsonb", nullable: false),
                    Address_Street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address_Street2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address_City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address_State = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    Address_ZipCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Children = table.Column<string>(type: "jsonb", nullable: false),
                    CurrentKehila = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ShabbosShul = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Budget = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Budget_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true, defaultValue: "USD"),
                    MinBedrooms = table.Column<int>(type: "integer", nullable: true),
                    MinBathrooms = table.Column<decimal>(type: "numeric(3,1)", nullable: true),
                    PreferredCities = table.Column<string>(type: "jsonb", nullable: false),
                    RequiredFeatures = table.Column<string>(type: "jsonb", nullable: false),
                    ShulProximity = table.Column<string>(type: "jsonb", nullable: true),
                    MoveTimeline = table.Column<int>(type: "integer", nullable: true),
                    EmploymentStatus = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    HousingNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DownPayment = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    DownPayment_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true, defaultValue: "USD"),
                    MortgageInterestRate = table.Column<decimal>(type: "numeric(5,3)", nullable: true),
                    LoanTermYears = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
                    BoardReviewDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BoardDecision = table.Column<int>(type: "integer", nullable: true),
                    BoardDecisionNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    BoardReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applicants", x => x.ApplicantId);
                });

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Stage = table.Column<int>(type: "integer", nullable: false),
                    StageChangedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ContractPropertyId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContractPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ContractPrice_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true, defaultValue: "USD"),
                    ContractDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualClosingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MovedInStatus = table.Column<int>(type: "integer", nullable: true),
                    MovedInDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
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
                name: "IX_Applicants_CreatedDate",
                table: "Applicants",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_IsDeleted",
                table: "Applicants",
                column: "IsDeleted");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "Applicants");
        }
    }
}
