using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EducationPortal.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDetailsEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "lookups",
                table: "SecurityAuditEventTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 49, "UserDetailsUpdated" },
                    { 50, "UserDetailsDeleted" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "lookups",
                table: "SecurityAuditEventTypes",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                schema: "lookups",
                table: "SecurityAuditEventTypes",
                keyColumn: "Id",
                keyValue: 50);
        }
    }
}
