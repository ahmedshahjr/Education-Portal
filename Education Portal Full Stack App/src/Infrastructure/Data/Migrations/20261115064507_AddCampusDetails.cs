using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EducationPortal.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCampusDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                schema: "schools",
                table: "Branches",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                schema: "schools",
                table: "Branches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                schema: "schools",
                table: "Branches",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                schema: "schools",
                table: "Branches",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrincipalUserDetailsId",
                schema: "schools",
                table: "Branches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchoolNumber",
                schema: "schools",
                table: "Branches",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortName",
                schema: "schools",
                table: "Branches",
                type: "character varying(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StateId",
                schema: "schools",
                table: "Branches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                schema: "schools",
                table: "Branches",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserTypes",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserDetails",
                schema: "schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserType = table.Column<int>(type: "integer", nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDetails", x => x.Id);
                    table.UniqueConstraint("AK_UserDetails_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_UserDetails_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDetails_UserTypes_UserType",
                        column: x => x.UserType,
                        principalSchema: "lookups",
                        principalTable: "UserTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "SecurityAuditEventTypes",
                columns: new[] { "Id", "Name" },
                values: new object[] { 46, "UserDetailsCreated" });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "UserTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Principal" },
                    { 2, "Teacher" },
                    { 3, "Staff" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_CityId",
                schema: "schools",
                table: "Branches",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_PrincipalUserDetailsId",
                schema: "schools",
                table: "Branches",
                column: "PrincipalUserDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_StateId",
                schema: "schools",
                table: "Branches",
                column: "StateId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Branches_CityNeedsState",
                schema: "schools",
                table: "Branches",
                sql: "\"CityId\" IS NULL OR \"StateId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserDetails_SchoolId_UserType",
                schema: "schools",
                table: "UserDetails",
                columns: new[] { "SchoolId", "UserType" });

            migrationBuilder.CreateIndex(
                name: "IX_UserDetails_UserType",
                schema: "schools",
                table: "UserDetails",
                column: "UserType");

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Cities_CityId",
                schema: "schools",
                table: "Branches",
                column: "CityId",
                principalSchema: "schools",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_States_StateId",
                schema: "schools",
                table: "Branches",
                column: "StateId",
                principalSchema: "schools",
                principalTable: "States",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_UserDetails_PrincipalUserDetailsId",
                schema: "schools",
                table: "Branches",
                column: "PrincipalUserDetailsId",
                principalSchema: "schools",
                principalTable: "UserDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Cities_CityId",
                schema: "schools",
                table: "Branches");

            migrationBuilder.DropForeignKey(
                name: "FK_Branches_States_StateId",
                schema: "schools",
                table: "Branches");

            migrationBuilder.DropForeignKey(
                name: "FK_Branches_UserDetails_PrincipalUserDetailsId",
                schema: "schools",
                table: "Branches");

            migrationBuilder.DropTable(
                name: "UserDetails",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "UserTypes",
                schema: "lookups");

            migrationBuilder.DropIndex(
                name: "IX_Branches_CityId",
                schema: "schools",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Branches_PrincipalUserDetailsId",
                schema: "schools",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Branches_StateId",
                schema: "schools",
                table: "Branches");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Branches_CityNeedsState",
                schema: "schools",
                table: "Branches");

            migrationBuilder.DeleteData(
                schema: "lookups",
                table: "SecurityAuditEventTypes",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DropColumn(
                name: "Address",
                schema: "schools",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "CityId",
                schema: "schools",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "Phone",
                schema: "schools",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                schema: "schools",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "PrincipalUserDetailsId",
                schema: "schools",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "SchoolNumber",
                schema: "schools",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "ShortName",
                schema: "schools",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "StateId",
                schema: "schools",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "Website",
                schema: "schools",
                table: "Branches");
        }
    }
}
