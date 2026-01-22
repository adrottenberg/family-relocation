using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyRelocation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed initial admin user
            migrationBuilder.Sql(@"
                INSERT INTO ""UserRoles"" (""Id"", ""CognitoUserId"", ""Email"", ""Role"", ""CreatedAt"", ""CreatedBy"")
                VALUES (
                    '00000000-0000-0000-0000-000000000001',
                    '4498a478-80d1-700e-c8b3-e0c539c1808a',
                    'adrottenberg@gmail.com',
                    'Admin',
                    NOW(),
                    'System Migration'
                )
                ON CONFLICT (""CognitoUserId"", ""Role"") DO NOTHING;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
