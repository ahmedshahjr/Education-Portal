using System;
using EducationPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EducationPortal.Infrastructure.Data.Migrations
{
    /// <summary>
    /// The academic foundations (ADR-019): courses, enrolments, assessments, scores and timetable slots in the
    /// <c>academics</c> schema, and the weekday lookup. Hand-written: check it against the model with
    /// <c>dotnet ef migrations list</c>.
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261119140000_AddAcademicFoundations")]
    public partial class AddAcademicFoundations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "academics");

            migrationBuilder.CreateTable(
                name: "Weekdays",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Weekdays", x => x.Id));

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "Weekdays",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Monday" },
                    { 2, "Tuesday" },
                    { 3, "Wednesday" },
                    { 4, "Thursday" },
                    { 5, "Friday" },
                    { 6, "Saturday" },
                    { 7, "Sunday" },
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    ClassOfferingId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TeacherUserDetailsId = table.Column<int>(type: "integer", nullable: false),
                    CampusId = table.Column<int>(type: "integer", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                    table.UniqueConstraint("AK_Courses_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_Courses_ClassOfferings_ClassOfferingId",
                        column: x => x.ClassOfferingId,
                        principalSchema: "schools",
                        principalTable: "ClassOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Courses_UserDetails_TeacherUserDetailsId",
                        column: x => x.TeacherUserDetailsId,
                        principalSchema: "schools",
                        principalTable: "UserDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Courses_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalSchema: "schools",
                        principalTable: "Campuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_Courses_SchoolId_TeacherUserDetailsId", schema: "academics", table: "Courses", columns: new[] { "SchoolId", "TeacherUserDetailsId" });
            migrationBuilder.CreateIndex(name: "IX_Courses_SchoolId_ClassOfferingId", schema: "academics", table: "Courses", columns: new[] { "SchoolId", "ClassOfferingId" });

            migrationBuilder.CreateTable(
                name: "Enrolments",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    StudentUserDetailsId = table.Column<int>(type: "integer", nullable: false),
                    ClassOfferingId = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrolments", x => x.Id);
                    table.UniqueConstraint("AK_Enrolments_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_Enrolments_UserDetails_StudentUserDetailsId",
                        column: x => x.StudentUserDetailsId,
                        principalSchema: "schools",
                        principalTable: "UserDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrolments_ClassOfferings_ClassOfferingId",
                        column: x => x.ClassOfferingId,
                        principalSchema: "schools",
                        principalTable: "ClassOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_SchoolId_StudentUserDetailsId_ClassOfferingId",
                schema: "academics",
                table: "Enrolments",
                columns: new[] { "SchoolId", "StudentUserDetailsId", "ClassOfferingId" },
                unique: true);

            migrationBuilder.CreateTable(
                name: "Assessments",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Weight = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    MaximumScore = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.Id);
                    table.UniqueConstraint("AK_Assessments_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_Assessments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalSchema: "academics",
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_Assessments_SchoolId_CourseId", schema: "academics", table: "Assessments", columns: new[] { "SchoolId", "CourseId" });

            migrationBuilder.CreateTable(
                name: "Scores",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    AssessmentId = table.Column<int>(type: "integer", nullable: false),
                    EnrolmentId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scores", x => x.Id);
                    table.UniqueConstraint("AK_Scores_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_Scores_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalSchema: "academics",
                        principalTable: "Assessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Scores_Enrolments_EnrolmentId",
                        column: x => x.EnrolmentId,
                        principalSchema: "academics",
                        principalTable: "Enrolments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_Scores_AssessmentId_EnrolmentId", schema: "academics", table: "Scores", columns: new[] { "AssessmentId", "EnrolmentId" }, unique: true);

            migrationBuilder.CreateTable(
                name: "TimetableSlots",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    CampusId = table.Column<int>(type: "integer", nullable: true),
                    Day = table.Column<int>(type: "integer", nullable: false),
                    Start = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    End = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Room = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimetableSlots", x => x.Id);
                    table.UniqueConstraint("AK_TimetableSlots_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_TimetableSlots_Courses_CourseId",
                        column: x => x.CourseId,
                        principalSchema: "academics",
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimetableSlots_Weekdays_Day",
                        column: x => x.Day,
                        principalSchema: "lookups",
                        principalTable: "Weekdays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimetableSlots_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalSchema: "schools",
                        principalTable: "Campuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_TimetableSlots_SchoolId_CourseId", schema: "academics", table: "TimetableSlots", columns: new[] { "SchoolId", "CourseId" });
            migrationBuilder.CreateIndex(name: "IX_TimetableSlots_SchoolId_Day", schema: "academics", table: "TimetableSlots", columns: new[] { "SchoolId", "Day" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "TimetableSlots", schema: "academics");
            migrationBuilder.DropTable(name: "Scores", schema: "academics");
            migrationBuilder.DropTable(name: "Assessments", schema: "academics");
            migrationBuilder.DropTable(name: "Enrolments", schema: "academics");
            migrationBuilder.DropTable(name: "Courses", schema: "academics");
            migrationBuilder.DropTable(name: "Weekdays", schema: "lookups");
        }
    }
}
