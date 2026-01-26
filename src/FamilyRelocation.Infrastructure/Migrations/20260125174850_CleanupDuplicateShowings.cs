using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyRelocation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CleanupDuplicateShowings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Cancel duplicate scheduled showings for the same PropertyMatch
            // Rule: Only one future scheduled showing allowed per PropertyMatch
            // Strategy: Keep the earliest scheduled showing, cancel the rest
            migrationBuilder.Sql(@"
                WITH RankedShowings AS (
                    SELECT
                        ""Id"",
                        ""PropertyMatchId"",
                        ""ScheduledDate"",
                        ""ScheduledTime"",
                        ""Status"",
                        ROW_NUMBER() OVER (
                            PARTITION BY ""PropertyMatchId""
                            ORDER BY ""ScheduledDate"" ASC, ""ScheduledTime"" ASC, ""CreatedAt"" ASC
                        ) as RowNum
                    FROM ""Showings""
                    WHERE ""Status"" = 0  -- 0 = Scheduled
                      AND ""ScheduledDate"" >= CURRENT_DATE
                )
                UPDATE ""Showings""
                SET ""Status"" = 2,  -- 2 = Cancelled
                    ""ModifiedAt"" = NOW()
                WHERE ""Id"" IN (
                    SELECT ""Id""
                    FROM RankedShowings
                    WHERE RowNum > 1
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Cannot reliably reverse this data migration
            // The cancelled showings cannot be automatically restored to Scheduled status
        }
    }
}
