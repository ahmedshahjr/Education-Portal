using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EducationPortal.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolPublicPageAuditEvents : Migration
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
                    { 51, "PublicPageProfileChanged" },
                    { 52, "PublicPagePublished" },
                    { 53, "PublicPageUnpublished" },
                    { 54, "SchoolCurriculaChanged" },
                    { 55, "SchoolFacilityChanged" },
                    { 56, "SchoolPhotoUploaded" },
                    { 57, "SchoolPhotoChanged" },
                    { 58, "SchoolPhotoDeleted" },
                    { 59, "ClassOfferingChanged" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "lookups",
                table: "SecurityAuditEventTypes",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                schema: "lookups",
                table: "SecurityAuditEventTypes",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                schema: "lookups",
                table: "SecurityAuditEventTypes",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                schema: "lookups",
                table: "SecurityAuditEventTypes",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                schema: "lookups",
                table: "SecurityAuditEventTypes",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                schema: "lookups",
                table: "SecurityAuditEventTypes",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                schema: "lookups",
                table: "SecurityAuditEventTypes",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                schema: "lookups",
                table: "SecurityAuditEventTypes",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                schema: "lookups",
                table: "SecurityAuditEventTypes",
                keyColumn: "Id",
                keyValue: 59);
        }
    }
}
