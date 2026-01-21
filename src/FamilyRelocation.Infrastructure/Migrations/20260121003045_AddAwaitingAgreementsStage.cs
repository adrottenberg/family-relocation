using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyRelocation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAwaitingAgreementsStage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Shift all existing stage values up by 1 to make room for AwaitingAgreements at position 0
            // Previous values: Searching=0, UnderContract=1, Closed=2, MovedIn=3, Paused=4
            // New values: AwaitingAgreements=0, Searching=1, UnderContract=2, Closed=3, MovedIn=4, Paused=5
            //
            // Existing Searching records (0) should become Searching (1) since they were already searching
            migrationBuilder.Sql(@"
                UPDATE ""HousingSearches""
                SET ""Stage"" = CASE ""Stage""
                    WHEN 4 THEN 5  -- Paused -> Paused
                    WHEN 3 THEN 4  -- MovedIn -> MovedIn
                    WHEN 2 THEN 3  -- Closed -> Closed
                    WHEN 1 THEN 2  -- UnderContract -> UnderContract
                    WHEN 0 THEN 1  -- Searching -> Searching (existing searches stay as Searching)
                    ELSE ""Stage"" + 1
                END
            ");

            // Add stage transition requirements for AwaitingAgreements (0) -> Searching (1)
            // using integer enum values since FromStage/ToStage are stored as integers
            migrationBuilder.Sql(@"
                INSERT INTO ""StageTransitionRequirements"" (""Id"", ""FromStage"", ""ToStage"", ""DocumentTypeId"", ""IsRequired"")
                SELECT
                    gen_random_uuid(),
                    0,  -- AwaitingAgreements
                    1,  -- Searching
                    dt.""Id"",
                    true
                FROM ""DocumentTypes"" dt
                WHERE dt.""IsActive"" = true
                AND NOT EXISTS (
                    SELECT 1 FROM ""StageTransitionRequirements"" str
                    WHERE str.""FromStage"" = 0
                    AND str.""ToStage"" = 1
                    AND str.""DocumentTypeId"" = dt.""Id""
                )
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove stage transition requirements for AwaitingAgreements -> Searching
            migrationBuilder.Sql(@"
                DELETE FROM ""StageTransitionRequirements""
                WHERE ""FromStage"" = 0 AND ""ToStage"" = 1
            ");

            // Shift stage values back down by 1
            // AwaitingAgreements records (0) become Searching (0) in the old schema
            migrationBuilder.Sql(@"
                UPDATE ""HousingSearches""
                SET ""Stage"" = CASE ""Stage""
                    WHEN 5 THEN 4  -- Paused -> Paused
                    WHEN 4 THEN 3  -- MovedIn -> MovedIn
                    WHEN 3 THEN 2  -- Closed -> Closed
                    WHEN 2 THEN 1  -- UnderContract -> UnderContract
                    WHEN 1 THEN 0  -- Searching -> Searching
                    WHEN 0 THEN 0  -- AwaitingAgreements -> Searching (merge into Searching)
                    ELSE ""Stage"" - 1
                END
            ");
        }
    }
}
