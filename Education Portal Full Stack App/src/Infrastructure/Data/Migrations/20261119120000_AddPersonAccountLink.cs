using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducationPortal.Infrastructure.Data.Migrations
{
    /// <summary>
    /// Links a login account to the person it belongs to (<c>UserDetailsId</c>). One account per person in a school: the
    /// unique index covers only accounts that have a person.
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261119120000_AddPersonAccountLink")]
    public partial class AddPersonAccountLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserDetailsId",
                schema: "identity",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SchoolId_UserDetailsId",
                schema: "identity",
                table: "AspNetUsers",
                columns: new[] { "SchoolId", "UserDetailsId" },
                unique: true,
                filter: "\"UserDetailsId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_SchoolId_UserDetailsId",
                schema: "identity",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UserDetailsId",
                schema: "identity",
                table: "AspNetUsers");
        }
    }
}
