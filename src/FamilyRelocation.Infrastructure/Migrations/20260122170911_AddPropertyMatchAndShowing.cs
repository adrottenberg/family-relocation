using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyRelocation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyMatchAndShowing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PropertyMatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HousingSearchId = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    MatchScore = table.Column<int>(type: "integer", nullable: false),
                    MatchDetails = table.Column<string>(type: "jsonb", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsAutoMatched = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyMatches_HousingSearches_HousingSearchId",
                        column: x => x.HousingSearchId,
                        principalTable: "HousingSearches",
                        principalColumn: "HousingSearchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyMatches_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Showings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyMatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ScheduledTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    BrokerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Showings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Showings_PropertyMatches_PropertyMatchId",
                        column: x => x.PropertyMatchId,
                        principalTable: "PropertyMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyMatches_HousingSearchId",
                table: "PropertyMatches",
                column: "HousingSearchId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyMatches_HousingSearchId_PropertyId",
                table: "PropertyMatches",
                columns: new[] { "HousingSearchId", "PropertyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyMatches_MatchScore",
                table: "PropertyMatches",
                column: "MatchScore");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyMatches_PropertyId",
                table: "PropertyMatches",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyMatches_Status",
                table: "PropertyMatches",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Showings_BrokerUserId",
                table: "Showings",
                column: "BrokerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Showings_PropertyMatchId",
                table: "Showings",
                column: "PropertyMatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Showings_ScheduledDate",
                table: "Showings",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_Showings_ScheduledDate_Status",
                table: "Showings",
                columns: new[] { "ScheduledDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Showings_Status",
                table: "Showings",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Showings");

            migrationBuilder.DropTable(
                name: "PropertyMatches");
        }
    }
}
