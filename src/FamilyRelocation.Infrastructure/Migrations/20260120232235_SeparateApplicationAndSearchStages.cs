using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FamilyRelocation.Infrastructure.Migrations
{
    /// <summary>
    /// Migration to separate ApplicationStatus (on Applicant) from HousingSearchStage (on HousingSearch).
    ///
    /// Before: HousingSearchStage = Submitted, BoardApproved, Rejected, HouseHunting, UnderContract, Closed, MovedIn, Paused
    /// After:
    ///   - ApplicationStatus (on Applicant) = Submitted, Approved, Rejected
    ///   - HousingSearchStage (on HousingSearch) = Searching, UnderContract, Closed, MovedIn, Paused
    /// </summary>
    public partial class SeparateApplicationAndSearchStages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add Status column to Applicants (temporarily nullable for data migration)
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Applicants",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            // Step 2: Migrate Status based on existing HousingSearch stage
            // Old Stage enum values (as integers):
            // Submitted = 0, BoardApproved = 1, Rejected = 2, HouseHunting = 3, UnderContract = 4, Closed = 5, MovedIn = 6, Paused = 7
            migrationBuilder.Sql(@"
                UPDATE ""Applicants"" a
                SET ""Status"" = CASE
                    WHEN EXISTS (
                        SELECT 1 FROM ""HousingSearches"" hs
                        WHERE hs.""ApplicantId"" = a.""ApplicantId""
                        AND hs.""Stage"" IN (1, 3, 4, 5, 6, 7)  -- BoardApproved, HouseHunting, UnderContract, Closed, MovedIn, Paused
                    ) THEN 'Approved'
                    WHEN EXISTS (
                        SELECT 1 FROM ""HousingSearches"" hs
                        WHERE hs.""ApplicantId"" = a.""ApplicantId""
                        AND hs.""Stage"" = 2  -- Rejected
                    ) THEN 'Rejected'
                    ELSE 'Submitted'
                END
            ");

            // Step 3: Set Status for any remaining applicants without HousingSearch
            migrationBuilder.Sql(@"
                UPDATE ""Applicants""
                SET ""Status"" = 'Submitted'
                WHERE ""Status"" IS NULL
            ");

            // Step 4: Make Status column required
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Applicants",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Submitted",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            // Step 5: Update HousingSearch Stage values to new enum
            // Old: Submitted=0, BoardApproved=1, Rejected=2, HouseHunting=3, UnderContract=4, Closed=5, MovedIn=6, Paused=7
            // New: Searching=0, UnderContract=1, Closed=2, MovedIn=3, Paused=4
            migrationBuilder.Sql(@"
                UPDATE ""HousingSearches""
                SET ""Stage"" = CASE ""Stage""
                    WHEN 3 THEN 0  -- HouseHunting -> Searching
                    WHEN 1 THEN 0  -- BoardApproved -> Searching (they're ready to search)
                    WHEN 4 THEN 1  -- UnderContract -> UnderContract
                    WHEN 5 THEN 2  -- Closed -> Closed
                    WHEN 6 THEN 3  -- MovedIn -> MovedIn
                    WHEN 7 THEN 4  -- Paused -> Paused
                    ELSE 0  -- Default to Searching (covers Submitted stage)
                END
            ");

            // Step 6: Delete housing searches for rejected applicants (they shouldn't have housing searches)
            migrationBuilder.Sql(@"
                DELETE FROM ""HousingSearches"" hs
                WHERE EXISTS (
                    SELECT 1 FROM ""Applicants"" a
                    WHERE a.""ApplicantId"" = hs.""ApplicantId""
                    AND a.""Status"" = 'Rejected'
                )
            ");

            // Step 7: Delete housing searches for submitted applicants (they're not approved yet)
            migrationBuilder.Sql(@"
                DELETE FROM ""HousingSearches"" hs
                WHERE EXISTS (
                    SELECT 1 FROM ""Applicants"" a
                    WHERE a.""ApplicantId"" = hs.""ApplicantId""
                    AND a.""Status"" = 'Submitted'
                )
            ");

            // Step 8: Drop the old unique index on HousingSearches.ApplicantId
            migrationBuilder.DropIndex(
                name: "IX_HousingSearches_ApplicantId",
                table: "HousingSearches");

            // Step 9: Delete obsolete stage transition requirements (BoardApproved -> HouseHunting)
            migrationBuilder.DeleteData(
                table: "StageTransitionRequirements",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccc01"));

            migrationBuilder.DeleteData(
                table: "StageTransitionRequirements",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccc02"));

            // Step 10: Create non-unique index on HousingSearches.ApplicantId (one-to-many relationship)
            migrationBuilder.CreateIndex(
                name: "IX_HousingSearches_ApplicantId",
                table: "HousingSearches",
                column: "ApplicantId");

            // Step 11: Create index on Applicants.Status for filtering
            migrationBuilder.CreateIndex(
                name: "IX_Applicants_Status",
                table: "Applicants",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Note: Down migration cannot fully restore the original state
            // because we've merged/deleted data during the Up migration.
            // This is a best-effort rollback.

            migrationBuilder.DropIndex(
                name: "IX_HousingSearches_ApplicantId",
                table: "HousingSearches");

            migrationBuilder.DropIndex(
                name: "IX_Applicants_Status",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Applicants");

            // Restore stage transition requirements seed data
            migrationBuilder.InsertData(
                table: "StageTransitionRequirements",
                columns: new[] { "Id", "DocumentTypeId", "FromStage", "IsRequired", "ToStage" },
                values: new object[,]
                {
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccc01"), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01"), 1, true, 3 },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccc02"), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02"), 1, true, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_HousingSearches_ApplicantId",
                table: "HousingSearches",
                column: "ApplicantId",
                unique: true);

            // Note: HousingSearch.Stage values are not rolled back as that would require
            // complex logic to determine original values
        }
    }
}
