using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyRelocation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFollowUpReminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FollowUpReminders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Normal"),
                    EntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Open"),
                    SendEmailNotification = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SnoozedUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SnoozeCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowUpReminders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FollowUpReminders_AssignedToUserId",
                table: "FollowUpReminders",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowUpReminders_DueDate",
                table: "FollowUpReminders",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_FollowUpReminders_EntityType_EntityId",
                table: "FollowUpReminders",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_FollowUpReminders_Priority",
                table: "FollowUpReminders",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_FollowUpReminders_Status",
                table: "FollowUpReminders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FollowUpReminders_Status_DueDate",
                table: "FollowUpReminders",
                columns: new[] { "Status", "DueDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FollowUpReminders");
        }
    }
}
