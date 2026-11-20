using System;
using EducationPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EducationPortal.Infrastructure.Data.Migrations
{
    /// <summary>
    /// Adds the Parent user type (number 5), the links between a parent and their students (<c>schools.ParentStudents</c>)
    /// and the school roles given to login accounts (<c>access.AccountRoles</c>). Hand-written: check it against the model
    /// with <c>dotnet ef migrations list</c>.
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261119130000_AddParentStudentsAndAccountRoles")]
    public partial class AddParentStudentsAndAccountRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "lookups",
                table: "UserTypes",
                columns: new[] { "Id", "Name" },
                values: new object[] { 5, "Parent" });

            migrationBuilder.CreateTable(
                name: "ParentStudents",
                schema: "schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    ParentUserDetailsId = table.Column<int>(type: "integer", nullable: false),
                    StudentUserDetailsId = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentStudents", x => x.Id);
                    table.UniqueConstraint("AK_ParentStudents_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_ParentStudents_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParentStudents_UserDetails_ParentUserDetailsId",
                        column: x => x.ParentUserDetailsId,
                        principalSchema: "schools",
                        principalTable: "UserDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParentStudents_UserDetails_StudentUserDetailsId",
                        column: x => x.StudentUserDetailsId,
                        principalSchema: "schools",
                        principalTable: "UserDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParentStudents_SchoolId_ParentUserDetailsId_StudentUserDetailsId",
                schema: "schools",
                table: "ParentStudents",
                columns: new[] { "SchoolId", "ParentUserDetailsId", "StudentUserDetailsId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParentStudents_SchoolId_StudentUserDetailsId",
                schema: "schools",
                table: "ParentStudents",
                columns: new[] { "SchoolId", "StudentUserDetailsId" });

            migrationBuilder.CreateTable(
                name: "AccountRoles",
                schema: "access",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    SchoolRoleId = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountRoles", x => x.Id);
                    table.UniqueConstraint("AK_AccountRoles_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_AccountRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountRoles_SchoolRoles_SchoolRoleId",
                        column: x => x.SchoolRoleId,
                        principalSchema: "access",
                        principalTable: "SchoolRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountRoles_SchoolId_UserId_SchoolRoleId",
                schema: "access",
                table: "AccountRoles",
                columns: new[] { "SchoolId", "UserId", "SchoolRoleId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AccountRoles", schema: "access");

            migrationBuilder.DropTable(name: "ParentStudents", schema: "schools");

            migrationBuilder.DeleteData(
                schema: "lookups",
                table: "UserTypes",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
