using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FamilyRelocation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurableDocumentSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrokerAgreementDocumentUrl",
                table: "HousingSearches");

            migrationBuilder.DropColumn(
                name: "BrokerAgreementSignedDate",
                table: "HousingSearches");

            migrationBuilder.DropColumn(
                name: "CommunityTakanosDocumentUrl",
                table: "HousingSearches");

            migrationBuilder.DropColumn(
                name: "CommunityTakanosSignedDate",
                table: "HousingSearches");

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsSystemType = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicantDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UploadedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicantDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicantDocuments_Applicants_ApplicantId",
                        column: x => x.ApplicantId,
                        principalTable: "Applicants",
                        principalColumn: "ApplicantId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicantDocuments_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StageTransitionRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStage = table.Column<int>(type: "integer", nullable: false),
                    ToStage = table.Column<int>(type: "integer", nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageTransitionRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StageTransitionRequirements_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "DocumentTypes",
                columns: new[] { "Id", "CreatedAt", "Description", "DisplayName", "IsActive", "IsSystemType", "ModifiedAt", "Name" },
                values: new object[,]
                {
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Agreement to work with the community's broker", "Broker Agreement", true, true, null, "BrokerAgreement" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Community guidelines and rules agreement", "Community Takanos", true, true, null, "CommunityTakanos" }
                });

            migrationBuilder.InsertData(
                table: "StageTransitionRequirements",
                columns: new[] { "Id", "DocumentTypeId", "FromStage", "IsRequired", "ToStage" },
                values: new object[,]
                {
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccc01"), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01"), 1, true, 3 },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccc02"), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02"), 1, true, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicantDocuments_ApplicantId",
                table: "ApplicantDocuments",
                column: "ApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicantDocuments_ApplicantId_DocumentTypeId",
                table: "ApplicantDocuments",
                columns: new[] { "ApplicantId", "DocumentTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicantDocuments_DocumentTypeId",
                table: "ApplicantDocuments",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicantDocuments_UploadedAt",
                table: "ApplicantDocuments",
                column: "UploadedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_IsActive",
                table: "DocumentTypes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_Name",
                table: "DocumentTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StageTransitionRequirements_DocumentTypeId",
                table: "StageTransitionRequirements",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_StageTransitionRequirements_FromStage_ToStage",
                table: "StageTransitionRequirements",
                columns: new[] { "FromStage", "ToStage" });

            migrationBuilder.CreateIndex(
                name: "IX_StageTransitionRequirements_FromStage_ToStage_DocumentTypeId",
                table: "StageTransitionRequirements",
                columns: new[] { "FromStage", "ToStage", "DocumentTypeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicantDocuments");

            migrationBuilder.DropTable(
                name: "StageTransitionRequirements");

            migrationBuilder.DropTable(
                name: "DocumentTypes");

            migrationBuilder.AddColumn<string>(
                name: "BrokerAgreementDocumentUrl",
                table: "HousingSearches",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

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

            migrationBuilder.AddColumn<DateTime>(
                name: "CommunityTakanosSignedDate",
                table: "HousingSearches",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
