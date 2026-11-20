using System;
using EducationPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EducationPortal.Infrastructure.Data.Migrations
{
    /// <summary>
    /// Grading scales and their bands (ADR-019) in the <c>academics</c> schema. Hand-written: check it against the model with
    /// <c>dotnet ef migrations list</c>.
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261119150000_AddGradingScales")]
    public partial class AddGradingScales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GradingScales",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradingScales", x => x.Id);
                    table.UniqueConstraint("AK_GradingScales_PublicId", x => x.PublicId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GradingScales_SchoolId_Name",
                schema: "academics",
                table: "GradingScales",
                columns: new[] { "SchoolId", "Name" },
                unique: true);

            migrationBuilder.CreateTable(
                name: "GradingBands",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    GradingScaleId = table.Column<int>(type: "integer", nullable: false),
                    MinPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    MaxPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Letter = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Points = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradingBands", x => x.Id);
                    table.UniqueConstraint("AK_GradingBands_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_GradingBands_GradingScales_GradingScaleId",
                        column: x => x.GradingScaleId,
                        principalSchema: "academics",
                        principalTable: "GradingScales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GradingBands_SchoolId_GradingScaleId",
                schema: "academics",
                table: "GradingBands",
                columns: new[] { "SchoolId", "GradingScaleId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GradingBands", schema: "academics");

            migrationBuilder.DropTable(name: "GradingScales", schema: "academics");
        }
    }
}
