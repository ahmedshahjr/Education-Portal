using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducationPortal.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class OptionalSchoolDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Domain",
                schema: "schools",
                table: "Schools",
                type: "character varying(253)",
                maxLength: 253,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(253)",
                oldMaxLength: 253);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Domain",
                schema: "schools",
                table: "Schools",
                type: "character varying(253)",
                maxLength: 253,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(253)",
                oldMaxLength: 253,
                oldNullable: true);
        }
    }
}
