using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyRelocation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityLogCommunicationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ActivityLogs",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "ActivityLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FollowUpReminderId",
                table: "ActivityLogs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Outcome",
                table: "ActivityLogs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "ActivityLogs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "System");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_Type",
                table: "ActivityLogs",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ActivityLogs_Type",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "FollowUpReminderId",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "Outcome",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "ActivityLogs");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ActivityLogs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);
        }
    }
}
