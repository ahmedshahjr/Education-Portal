using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducationPortal.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AllowSwitchingOffCoreModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Modules_CoreIsActive",
                schema: "modules",
                table: "Modules");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Modules_CoreIsActive",
                schema: "modules",
                table: "Modules",
                sql: "NOT \"IsCore\" OR \"IsActive\"");
        }
    }
}
