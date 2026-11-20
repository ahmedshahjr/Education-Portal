using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EducationPortal.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UserCampusesAndRoleRights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RolePermissions_NeedView",
                schema: "access",
                table: "RolePermissions");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "access",
                table: "SchoolRoles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystem",
                schema: "access",
                table: "SchoolRoles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanApprove",
                schema: "access",
                table: "RolePermissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanExport",
                schema: "access",
                table: "RolePermissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "UserCampuses",
                schema: "access",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    UserDetailsId = table.Column<int>(type: "integer", nullable: false),
                    CampusId = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCampuses", x => x.Id);
                    table.UniqueConstraint("AK_UserCampuses_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_UserCampuses_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalSchema: "schools",
                        principalTable: "Campuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCampuses_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCampuses_UserDetails_UserDetailsId",
                        column: x => x.UserDetailsId,
                        principalSchema: "schools",
                        principalTable: "UserDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_RolePermissions_NeedView",
                schema: "access",
                table: "RolePermissions",
                sql: "\"CanView\" OR NOT (\"CanCreate\" OR \"CanEdit\" OR \"CanDelete\" OR \"CanApprove\" OR \"CanExport\")");

            // Schools made before this migration already have the built-in roles: mark them as system roles, and give the
            // Principal the new rights wherever it can view, as a new school's Principal gets them (BuiltInRoles).
            migrationBuilder.Sql(
                "UPDATE access.\"SchoolRoles\" SET \"IsSystem\" = TRUE WHERE \"Name\" IN ('Principal', 'Teacher', 'Student', 'Parent');");
            migrationBuilder.Sql(
                "UPDATE access.\"RolePermissions\" p SET \"CanApprove\" = p.\"CanView\", \"CanExport\" = p.\"CanView\" " +
                "FROM access.\"SchoolRoles\" r WHERE r.\"Id\" = p.\"SchoolRoleId\" AND r.\"Name\" = 'Principal';");

            migrationBuilder.CreateIndex(
                name: "IX_UserCampuses_CampusId",
                schema: "access",
                table: "UserCampuses",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCampuses_SchoolId",
                schema: "access",
                table: "UserCampuses",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCampuses_UserDetailsId_CampusId",
                schema: "access",
                table: "UserCampuses",
                columns: new[] { "UserDetailsId", "CampusId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCampuses",
                schema: "access");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RolePermissions_NeedView",
                schema: "access",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "access",
                table: "SchoolRoles");

            migrationBuilder.DropColumn(
                name: "IsSystem",
                schema: "access",
                table: "SchoolRoles");

            migrationBuilder.DropColumn(
                name: "CanApprove",
                schema: "access",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "CanExport",
                schema: "access",
                table: "RolePermissions");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RolePermissions_NeedView",
                schema: "access",
                table: "RolePermissions",
                sql: "\"CanView\" OR NOT (\"CanCreate\" OR \"CanEdit\" OR \"CanDelete\")");
        }
    }
}
