using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducationPortal.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RolePermissionCampus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_SchoolRoleId_ModuleId",
                schema: "access",
                table: "RolePermissions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RolePermissions_NeedView",
                schema: "access",
                table: "RolePermissions");

            migrationBuilder.AddColumn<bool>(
                name: "AllCampuses",
                schema: "access",
                table: "RolePermissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CampusId",
                schema: "access",
                table: "RolePermissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Backfill (ADR-022): existing rights apply to every campus, and are recorded on the school's main campus (Kind 1).
            migrationBuilder.Sql(
                """
                UPDATE access."RolePermissions" p
                SET "CampusId" = c."Id", "AllCampuses" = TRUE
                FROM schools."Campuses" c
                WHERE c."SchoolId" = p."SchoolId" AND c."Kind" = 1;

                -- Stop loudly rather than fail on the foreign key below: every role right must have a main campus.
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM access."RolePermissions" WHERE "CampusId" = 0) THEN
                        RAISE EXCEPTION 'RolePermissions rows without a main campus; repair the schools first (ADR-022)';
                    END IF;
                END $$;
                """);

            migrationBuilder.AddColumn<bool>(
                name: "CanViewHistory",
                schema: "access",
                table: "RolePermissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_CampusId",
                schema: "access",
                table: "RolePermissions",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_SchoolRoleId_ModuleId_CampusId",
                schema: "access",
                table: "RolePermissions",
                columns: new[] { "SchoolRoleId", "ModuleId", "CampusId" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_RolePermissions_NeedView",
                schema: "access",
                table: "RolePermissions",
                sql: "\"CanView\" OR NOT (\"CanCreate\" OR \"CanEdit\" OR \"CanDelete\" OR \"CanApprove\" OR \"CanExport\" OR \"CanViewHistory\")");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Campuses_CampusId",
                schema: "access",
                table: "RolePermissions",
                column: "CampusId",
                principalSchema: "schools",
                principalTable: "Campuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Campuses_CampusId",
                schema: "access",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_CampusId",
                schema: "access",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_SchoolRoleId_ModuleId_CampusId",
                schema: "access",
                table: "RolePermissions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RolePermissions_NeedView",
                schema: "access",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "AllCampuses",
                schema: "access",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "CampusId",
                schema: "access",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "CanViewHistory",
                schema: "access",
                table: "RolePermissions");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_SchoolRoleId_ModuleId",
                schema: "access",
                table: "RolePermissions",
                columns: new[] { "SchoolRoleId", "ModuleId" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_RolePermissions_NeedView",
                schema: "access",
                table: "RolePermissions",
                sql: "\"CanView\" OR NOT (\"CanCreate\" OR \"CanEdit\" OR \"CanDelete\" OR \"CanApprove\" OR \"CanExport\")");
        }
    }
}
