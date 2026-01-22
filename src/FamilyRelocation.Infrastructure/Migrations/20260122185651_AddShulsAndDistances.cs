using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyRelocation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShulsAndDistances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Shuls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Street2 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    ZipCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    Rabbi = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Denomination = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shuls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PropertyShulDistances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShulId = table.Column<Guid>(type: "uuid", nullable: false),
                    DistanceMiles = table.Column<double>(type: "double precision", nullable: false),
                    WalkingTimeMinutes = table.Column<int>(type: "integer", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyShulDistances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyShulDistances_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyShulDistances_Shuls_ShulId",
                        column: x => x.ShulId,
                        principalTable: "Shuls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyShulDistances_PropertyId",
                table: "PropertyShulDistances",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyShulDistances_PropertyId_ShulId",
                table: "PropertyShulDistances",
                columns: new[] { "PropertyId", "ShulId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyShulDistances_ShulId",
                table: "PropertyShulDistances",
                column: "ShulId");

            migrationBuilder.CreateIndex(
                name: "IX_Shuls_IsActive",
                table: "Shuls",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Shuls_Name",
                table: "Shuls",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PropertyShulDistances");

            migrationBuilder.DropTable(
                name: "Shuls");
        }
    }
}
