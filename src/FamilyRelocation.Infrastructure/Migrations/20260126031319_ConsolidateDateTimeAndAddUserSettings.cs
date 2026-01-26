using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyRelocation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateDateTimeAndAddUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add the new ScheduledDateTime column to Showings (nullable initially)
            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledDateTime",
                table: "Showings",
                type: "timestamp with time zone",
                nullable: true);

            // Step 2: Migrate existing Showings data - combine ScheduledDate and ScheduledTime into ScheduledDateTime
            migrationBuilder.Sql(@"
                UPDATE ""Showings""
                SET ""ScheduledDateTime"" = (""ScheduledDate""::timestamp + ""ScheduledTime"") AT TIME ZONE 'America/New_York' AT TIME ZONE 'UTC'
                WHERE ""ScheduledDate"" IS NOT NULL AND ""ScheduledTime"" IS NOT NULL;
            ");

            // Step 3: Make ScheduledDateTime NOT NULL after data migration
            migrationBuilder.AlterColumn<DateTime>(
                name: "ScheduledDateTime",
                table: "Showings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // Step 4: Drop old indexes for Showings
            migrationBuilder.DropIndex(
                name: "IX_Showings_ScheduledDate",
                table: "Showings");

            migrationBuilder.DropIndex(
                name: "IX_Showings_ScheduledDate_Status",
                table: "Showings");

            // Step 5: Drop old Showings columns
            migrationBuilder.DropColumn(
                name: "ScheduledDate",
                table: "Showings");

            migrationBuilder.DropColumn(
                name: "ScheduledTime",
                table: "Showings");

            // Step 6: Handle FollowUpReminders - DueDate was already datetime, just combine with DueTime
            // First, update DueDate to include DueTime (if DueTime was set)
            migrationBuilder.Sql(@"
                UPDATE ""FollowUpReminders""
                SET ""DueDate"" = ""DueDate"" + COALESCE(""DueTime"", '14:00:00'::time)
                WHERE ""DueTime"" IS NOT NULL;
            ");

            // Step 7: Drop DueTime column from FollowUpReminders
            migrationBuilder.DropColumn(
                name: "DueTime",
                table: "FollowUpReminders");

            // Step 8: Rename DueDate to DueDateTime
            migrationBuilder.RenameColumn(
                name: "DueDate",
                table: "FollowUpReminders",
                newName: "DueDateTime");

            migrationBuilder.RenameIndex(
                name: "IX_FollowUpReminders_Status_DueDate",
                table: "FollowUpReminders",
                newName: "IX_FollowUpReminders_Status_DueDateTime");

            migrationBuilder.RenameIndex(
                name: "IX_FollowUpReminders_DueDate",
                table: "FollowUpReminders",
                newName: "IX_FollowUpReminders_DueDateTime");

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TimeZoneId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "America/New_York"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Showings_ScheduledDateTime",
                table: "Showings",
                column: "ScheduledDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_Showings_ScheduledDateTime_Status",
                table: "Showings",
                columns: new[] { "ScheduledDateTime", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_UserId",
                table: "UserSettings",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropIndex(
                name: "IX_Showings_ScheduledDateTime",
                table: "Showings");

            migrationBuilder.DropIndex(
                name: "IX_Showings_ScheduledDateTime_Status",
                table: "Showings");

            migrationBuilder.DropColumn(
                name: "ScheduledDateTime",
                table: "Showings");

            migrationBuilder.RenameColumn(
                name: "DueDateTime",
                table: "FollowUpReminders",
                newName: "DueDate");

            migrationBuilder.RenameIndex(
                name: "IX_FollowUpReminders_Status_DueDateTime",
                table: "FollowUpReminders",
                newName: "IX_FollowUpReminders_Status_DueDate");

            migrationBuilder.RenameIndex(
                name: "IX_FollowUpReminders_DueDateTime",
                table: "FollowUpReminders",
                newName: "IX_FollowUpReminders_DueDate");

            migrationBuilder.AddColumn<DateOnly>(
                name: "ScheduledDate",
                table: "Showings",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "ScheduledTime",
                table: "Showings",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "DueTime",
                table: "FollowUpReminders",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Showings_ScheduledDate",
                table: "Showings",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_Showings_ScheduledDate_Status",
                table: "Showings",
                columns: new[] { "ScheduledDate", "Status" });
        }
    }
}
