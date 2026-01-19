using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyRelocation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Applicants",
                columns: table => new
                {
                    ApplicantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Address_Street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address_Street2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address_City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address_State = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    Address_ZipCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CurrentKehila = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ShabbosShul = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BoardDecision = table.Column<int>(type: "integer", nullable: true),
                    BoardDecisionNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    BoardReviewDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BoardReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Children = table.Column<string>(type: "jsonb", nullable: true),
                    Husband = table.Column<string>(type: "jsonb", nullable: false),
                    Wife = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applicants", x => x.ApplicantId);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OldValues = table.Column<string>(type: "jsonb", nullable: true),
                    NewValues = table.Column<string>(type: "jsonb", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HousingSearches",
                columns: table => new
                {
                    HousingSearchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Stage = table.Column<int>(type: "integer", nullable: false),
                    StageChangedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MovedInStatus = table.Column<int>(type: "integer", nullable: true),
                    MovedInDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    BrokerAgreementDocumentUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BrokerAgreementSignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CommunityTakanosDocumentUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CommunityTakanosSignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CurrentContract = table.Column<string>(type: "jsonb", nullable: true),
                    FailedContracts = table.Column<string>(type: "jsonb", nullable: true),
                    Preferences = table.Column<string>(type: "jsonb", nullable: true)
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
                name: "IX_Applicants_CreatedDate",
                table: "Applicants",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_IsDeleted",
                table: "Applicants",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityId",
                table: "AuditLogs",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType",
                table: "AuditLogs",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

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
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "HousingSearches");

            migrationBuilder.DropTable(
                name: "Applicants");
        }
    }
}
