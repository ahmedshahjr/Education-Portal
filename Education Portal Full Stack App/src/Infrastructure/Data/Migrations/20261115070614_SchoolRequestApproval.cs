using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EducationPortal.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SchoolRequestApproval : Migration
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
                    { 47, "SchoolRequestApproved" },
                    { 48, "SchoolRequestRejected" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "lookups",
                table: "SecurityAuditEventTypes",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                schema: "lookups",
                table: "SecurityAuditEventTypes",
                keyColumn: "Id",
                keyValue: 48);
        }
    }
}
