using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducationPortal.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DefaultModuleGrants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SubscriptionId",
                schema: "modules",
                table: "TenantModules",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<bool>(
                name: "IsDefaultEnabled",
                schema: "modules",
                table: "Modules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Core modules were always on for every school, so they stay on by default for a school that registers.
            migrationBuilder.Sql("UPDATE modules.\"Modules\" SET \"IsDefaultEnabled\" = TRUE WHERE \"IsCore\" = TRUE;");

            migrationBuilder.CreateIndex(
                name: "IX_TenantModules_DefaultGrantOnce",
                schema: "modules",
                table: "TenantModules",
                columns: new[] { "SchoolId", "ModuleId", "BranchId" },
                unique: true,
                filter: "\"SubscriptionId\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TenantModules_DefaultGrantOnce",
                schema: "modules",
                table: "TenantModules");

            migrationBuilder.DropColumn(
                name: "IsDefaultEnabled",
                schema: "modules",
                table: "Modules");

            migrationBuilder.AlterColumn<int>(
                name: "SubscriptionId",
                schema: "modules",
                table: "TenantModules",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
