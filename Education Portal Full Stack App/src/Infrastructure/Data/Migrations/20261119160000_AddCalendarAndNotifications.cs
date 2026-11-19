using System;
using EducationPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EducationPortal.Infrastructure.Data.Migrations
{
    /// <summary>
    /// The school calendar and notifications (ADR-019): school events and their notifications in the new
    /// <c>communication</c> schema, and the event audience lookup. Hand-written: check it against the model with
    /// <c>dotnet ef migrations list</c>.
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261119160000_AddCalendarAndNotifications")]
    public partial class AddCalendarAndNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "communication");

            migrationBuilder.CreateTable(
                name: "EventAudiences",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_EventAudiences", x => x.Id));

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "EventAudiences",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "All" },
                    { 2, "Staff" },
                    { 3, "Parents" },
                    { 4, "Students" },
                });

            migrationBuilder.CreateTable(
                name: "SchoolEvents",
                schema: "communication",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    StartsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Audience = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolEvents", x => x.Id);
                    table.UniqueConstraint("AK_SchoolEvents_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_SchoolEvents_EventAudiences_Audience",
                        column: x => x.Audience,
                        principalSchema: "lookups",
                        principalTable: "EventAudiences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolEvents_SchoolId_StartsAt",
                schema: "communication",
                table: "SchoolEvents",
                columns: new[] { "SchoolId", "StartsAt" });

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "communication",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    RecipientAccountId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    SchoolEventId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.UniqueConstraint("AK_Notifications_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_Notifications_SchoolEvents_SchoolEventId",
                        column: x => x.SchoolEventId,
                        principalSchema: "communication",
                        principalTable: "SchoolEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SchoolId_RecipientAccountId_Created",
                schema: "communication",
                table: "Notifications",
                columns: new[] { "SchoolId", "RecipientAccountId", "Created" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Notifications", schema: "communication");

            migrationBuilder.DropTable(name: "SchoolEvents", schema: "communication");

            migrationBuilder.DropTable(name: "EventAudiences", schema: "lookups");
        }
    }
}
