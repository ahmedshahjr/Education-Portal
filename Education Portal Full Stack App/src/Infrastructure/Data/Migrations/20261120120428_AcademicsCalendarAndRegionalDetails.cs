using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EducationPortal.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AcademicsCalendarAndRegionalDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "academics");

            migrationBuilder.EnsureSchema(
                name: "communication");

            migrationBuilder.AddColumn<int>(
                name: "AcademicYearStartMonth",
                schema: "schools",
                table: "Schools",
                type: "integer",
                nullable: false,
                defaultValue: 8);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                schema: "schools",
                table: "Schools",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                schema: "schools",
                table: "Schools",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "schools",
                table: "Schools",
                type: "character varying(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocalName",
                schema: "schools",
                table: "Schools",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                schema: "schools",
                table: "Schools",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationAuthority",
                schema: "schools",
                table: "Schools",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                schema: "schools",
                table: "Schools",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxNumber",
                schema: "schools",
                table: "Schools",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                schema: "schools",
                table: "Schools",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrencyDecimals",
                schema: "schools",
                table: "Countries",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<string>(
                name: "PhonePattern",
                schema: "schools",
                table: "Countries",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WeekStartDay",
                schema: "schools",
                table: "Countries",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int[]>(
                name: "WeekendDays",
                schema: "schools",
                table: "Countries",
                type: "integer[]",
                nullable: false,
                // Existing countries get the common Saturday and Sunday weekend (DayOfWeek 6 and 0), as new ones do.
                defaultValue: new[] { 6, 0 });

            migrationBuilder.AddColumn<int>(
                name: "GenderPolicy",
                schema: "schools",
                table: "Campuses",
                type: "integer",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<string>(
                name: "LocalName",
                schema: "schools",
                table: "Campuses",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AcademicYearStatuses",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicYearStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CalendarEventAudiences",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarEventAudiences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CalendarEventTypes",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarEventTypes", x => x.Id);
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
                        name: "FK_Courses_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalSchema: "schools",
                        principalTable: "Campuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                });

            migrationBuilder.CreateTable(
                name: "DaysOfWeek",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DaysOfWeek", x => x.Id);
                });

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
                        name: "FK_Enrolments_ClassOfferings_ClassOfferingId",
                        column: x => x.ClassOfferingId,
                        principalSchema: "schools",
                        principalTable: "ClassOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrolments_UserDetails_StudentUserDetailsId",
                        column: x => x.StudentUserDetailsId,
                        principalSchema: "schools",
                        principalTable: "UserDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventAudiences",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventAudiences", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "HolidayAudiences",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HolidayAudiences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HolidayTypes",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HolidayTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shifts",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    CampusId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
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
                    table.PrimaryKey("PK_Shifts", x => x.Id);
                    table.UniqueConstraint("AK_Shifts_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_Shifts_Times", "\"EndTime\" > \"StartTime\"");
                    table.ForeignKey(
                        name: "FK_Shifts_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalSchema: "schools",
                        principalTable: "Campuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Shifts_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Weekdays",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Weekdays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AcademicYears",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_AcademicYears", x => x.Id);
                    table.UniqueConstraint("AK_AcademicYears_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_AcademicYears_Dates", "\"EndDate\" >= \"StartDate\"");
                    table.ForeignKey(
                        name: "FK_AcademicYears_AcademicYearStatuses_Status",
                        column: x => x.Status,
                        principalSchema: "lookups",
                        principalTable: "AcademicYearStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AcademicYears_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CalendarEvents",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    CampusId = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EventDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Audience = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_CalendarEvents", x => x.Id);
                    table.UniqueConstraint("AK_CalendarEvents_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_CalendarEvents_EndNeedsStart", "\"EndTime\" IS NULL OR \"StartTime\" IS NOT NULL");
                    table.CheckConstraint("CK_CalendarEvents_Times", "\"EndTime\" IS NULL OR \"EndTime\" > \"StartTime\"");
                    table.ForeignKey(
                        name: "FK_CalendarEvents_CalendarEventAudiences_Audience",
                        column: x => x.Audience,
                        principalSchema: "lookups",
                        principalTable: "CalendarEventAudiences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalendarEvents_CalendarEventTypes_Type",
                        column: x => x.Type,
                        principalSchema: "lookups",
                        principalTable: "CalendarEventTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalendarEvents_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalSchema: "schools",
                        principalTable: "Campuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalendarEvents_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "Holidays",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    CampusId = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    AppliesTo = table.Column<int>(type: "integer", nullable: false),
                    IsTentative = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_Holidays", x => x.Id);
                    table.UniqueConstraint("AK_Holidays_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_Holidays_Dates", "\"EndDate\" >= \"StartDate\"");
                    table.ForeignKey(
                        name: "FK_Holidays_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalSchema: "schools",
                        principalTable: "Campuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Holidays_HolidayAudiences_AppliesTo",
                        column: x => x.AppliesTo,
                        principalSchema: "lookups",
                        principalTable: "HolidayAudiences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Holidays_HolidayTypes_Type",
                        column: x => x.Type,
                        principalSchema: "lookups",
                        principalTable: "HolidayTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Holidays_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                        name: "FK_TimetableSlots_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalSchema: "schools",
                        principalTable: "Campuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                });

            migrationBuilder.CreateTable(
                name: "Terms",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    AcademicYearId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
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
                    table.PrimaryKey("PK_Terms", x => x.Id);
                    table.UniqueConstraint("AK_Terms_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_Terms_Dates", "\"EndDate\" >= \"StartDate\"");
                    table.CheckConstraint("CK_Terms_Sequence", "\"Sequence\" BETWEEN 1 AND 12");
                    table.ForeignKey(
                        name: "FK_Terms_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalSchema: "academics",
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Terms_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "AcademicYearStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Planned" },
                    { 2, "Active" },
                    { 3, "Closed" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "CalendarEventAudiences",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "All" },
                    { 2, "Students" },
                    { 3, "Staff" },
                    { 4, "Parents" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "CalendarEventTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "ParentTeacherMeeting" },
                    { 2, "Sports" },
                    { 3, "Exam" },
                    { 4, "Function" },
                    { 5, "Other" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "DaysOfWeek",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 0, "Sunday" },
                    { 1, "Monday" },
                    { 2, "Tuesday" },
                    { 3, "Wednesday" },
                    { 4, "Thursday" },
                    { 5, "Friday" },
                    { 6, "Saturday" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "EventAudiences",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "All" },
                    { 2, "Staff" },
                    { 3, "Parents" },
                    { 4, "Students" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "HolidayAudiences",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Students" },
                    { 2, "Staff" },
                    { 3, "Both" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "HolidayTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Public" },
                    { 2, "Religious" },
                    { 3, "Vacation" },
                    { 4, "School" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "SecurityAuditEventTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 60, "AcademicYearChanged" },
                    { 61, "ShiftChanged" },
                    { 62, "SchoolCalendarChanged" },
                    { 63, "SchoolDetailsChanged" }
                });

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
                    { 7, "Sunday" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Schools_CityId",
                schema: "schools",
                table: "Schools",
                column: "CityId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Schools_AcademicYearStartMonth",
                schema: "schools",
                table: "Schools",
                sql: "\"AcademicYearStartMonth\" BETWEEN 1 AND 12");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_WeekStartDay",
                schema: "schools",
                table: "Countries",
                column: "WeekStartDay");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Countries_CurrencyDecimals",
                schema: "schools",
                table: "Countries",
                sql: "\"CurrencyDecimals\" BETWEEN 0 AND 4");

            migrationBuilder.CreateIndex(
                name: "IX_Campuses_GenderPolicy",
                schema: "schools",
                table: "Campuses",
                column: "GenderPolicy");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_SchoolId_Name",
                schema: "academics",
                table: "AcademicYears",
                columns: new[] { "SchoolId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_SingleCurrentPerSchool",
                schema: "academics",
                table: "AcademicYears",
                column: "SchoolId",
                unique: true,
                filter: "\"IsCurrent\" AND \"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_Status",
                schema: "academics",
                table: "AcademicYears",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_CourseId",
                schema: "academics",
                table: "Assessments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_SchoolId_CourseId",
                schema: "academics",
                table: "Assessments",
                columns: new[] { "SchoolId", "CourseId" });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_Audience",
                schema: "academics",
                table: "CalendarEvents",
                column: "Audience");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_CampusId",
                schema: "academics",
                table: "CalendarEvents",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_SchoolId_EventDate",
                schema: "academics",
                table: "CalendarEvents",
                columns: new[] { "SchoolId", "EventDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_Type",
                schema: "academics",
                table: "CalendarEvents",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CampusId",
                schema: "academics",
                table: "Courses",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_ClassOfferingId",
                schema: "academics",
                table: "Courses",
                column: "ClassOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_SchoolId_ClassOfferingId",
                schema: "academics",
                table: "Courses",
                columns: new[] { "SchoolId", "ClassOfferingId" });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_SchoolId_TeacherUserDetailsId",
                schema: "academics",
                table: "Courses",
                columns: new[] { "SchoolId", "TeacherUserDetailsId" });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_TeacherUserDetailsId",
                schema: "academics",
                table: "Courses",
                column: "TeacherUserDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_ClassOfferingId",
                schema: "academics",
                table: "Enrolments",
                column: "ClassOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_SchoolId_StudentUserDetailsId_ClassOfferingId",
                schema: "academics",
                table: "Enrolments",
                columns: new[] { "SchoolId", "StudentUserDetailsId", "ClassOfferingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_StudentUserDetailsId",
                schema: "academics",
                table: "Enrolments",
                column: "StudentUserDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_GradingBands_GradingScaleId",
                schema: "academics",
                table: "GradingBands",
                column: "GradingScaleId");

            migrationBuilder.CreateIndex(
                name: "IX_GradingBands_SchoolId_GradingScaleId",
                schema: "academics",
                table: "GradingBands",
                columns: new[] { "SchoolId", "GradingScaleId" });

            migrationBuilder.CreateIndex(
                name: "IX_GradingScales_SchoolId_Name",
                schema: "academics",
                table: "GradingScales",
                columns: new[] { "SchoolId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_AppliesTo",
                schema: "academics",
                table: "Holidays",
                column: "AppliesTo");

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_CampusId",
                schema: "academics",
                table: "Holidays",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_SchoolId_StartDate",
                schema: "academics",
                table: "Holidays",
                columns: new[] { "SchoolId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_Type",
                schema: "academics",
                table: "Holidays",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SchoolEventId",
                schema: "communication",
                table: "Notifications",
                column: "SchoolEventId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SchoolId_RecipientAccountId_Created",
                schema: "communication",
                table: "Notifications",
                columns: new[] { "SchoolId", "RecipientAccountId", "Created" });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolEvents_Audience",
                schema: "communication",
                table: "SchoolEvents",
                column: "Audience");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolEvents_SchoolId_StartsAt",
                schema: "communication",
                table: "SchoolEvents",
                columns: new[] { "SchoolId", "StartsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Scores_AssessmentId_EnrolmentId",
                schema: "academics",
                table: "Scores",
                columns: new[] { "AssessmentId", "EnrolmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Scores_EnrolmentId",
                schema: "academics",
                table: "Scores",
                column: "EnrolmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_CampusId_Name",
                schema: "academics",
                table: "Shifts",
                columns: new[] { "CampusId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_SchoolId",
                schema: "academics",
                table: "Shifts",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Terms_AcademicYearId_Sequence",
                schema: "academics",
                table: "Terms",
                columns: new[] { "AcademicYearId", "Sequence" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Terms_SchoolId",
                schema: "academics",
                table: "Terms",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableSlots_CampusId",
                schema: "academics",
                table: "TimetableSlots",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableSlots_CourseId",
                schema: "academics",
                table: "TimetableSlots",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableSlots_Day",
                schema: "academics",
                table: "TimetableSlots",
                column: "Day");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableSlots_SchoolId_CourseId",
                schema: "academics",
                table: "TimetableSlots",
                columns: new[] { "SchoolId", "CourseId" });

            migrationBuilder.CreateIndex(
                name: "IX_TimetableSlots_SchoolId_Day",
                schema: "academics",
                table: "TimetableSlots",
                columns: new[] { "SchoolId", "Day" });

            migrationBuilder.AddForeignKey(
                name: "FK_Campuses_SchoolGenderPolicies_GenderPolicy",
                schema: "schools",
                table: "Campuses",
                column: "GenderPolicy",
                principalSchema: "lookups",
                principalTable: "SchoolGenderPolicies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Countries_DaysOfWeek_WeekStartDay",
                schema: "schools",
                table: "Countries",
                column: "WeekStartDay",
                principalSchema: "lookups",
                principalTable: "DaysOfWeek",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_Cities_CityId",
                schema: "schools",
                table: "Schools",
                column: "CityId",
                principalSchema: "schools",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campuses_SchoolGenderPolicies_GenderPolicy",
                schema: "schools",
                table: "Campuses");

            migrationBuilder.DropForeignKey(
                name: "FK_Countries_DaysOfWeek_WeekStartDay",
                schema: "schools",
                table: "Countries");

            migrationBuilder.DropForeignKey(
                name: "FK_Schools_Cities_CityId",
                schema: "schools",
                table: "Schools");

            migrationBuilder.DropTable(
                name: "CalendarEvents",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "DaysOfWeek",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "GradingBands",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "Holidays",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "communication");

            migrationBuilder.DropTable(
                name: "Scores",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "Shifts",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "Terms",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "TimetableSlots",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "CalendarEventAudiences",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "CalendarEventTypes",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "GradingScales",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "HolidayAudiences",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "HolidayTypes",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "SchoolEvents",
                schema: "communication");

            migrationBuilder.DropTable(
                name: "Assessments",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "Enrolments",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "AcademicYears",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "Weekdays",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "EventAudiences",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "Courses",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "AcademicYearStatuses",
                schema: "lookups");

            migrationBuilder.DropIndex(
                name: "IX_Schools_CityId",
                schema: "schools",
                table: "Schools");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Schools_AcademicYearStartMonth",
                schema: "schools",
                table: "Schools");

            migrationBuilder.DropIndex(
                name: "IX_Countries_WeekStartDay",
                schema: "schools",
                table: "Countries");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Countries_CurrencyDecimals",
                schema: "schools",
                table: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_Campuses_GenderPolicy",
                schema: "schools",
                table: "Campuses");

            migrationBuilder.DeleteData(
                schema: "lookups",
                table: "SecurityAuditEventTypes",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                schema: "lookups",
                table: "SecurityAuditEventTypes",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                schema: "lookups",
                table: "SecurityAuditEventTypes",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                schema: "lookups",
                table: "SecurityAuditEventTypes",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DropColumn(
                name: "AcademicYearStartMonth",
                schema: "schools",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "Address",
                schema: "schools",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "CityId",
                schema: "schools",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "schools",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "LocalName",
                schema: "schools",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "Phone",
                schema: "schools",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "RegistrationAuthority",
                schema: "schools",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                schema: "schools",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "TaxNumber",
                schema: "schools",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "Website",
                schema: "schools",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "CurrencyDecimals",
                schema: "schools",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "PhonePattern",
                schema: "schools",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "WeekStartDay",
                schema: "schools",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "WeekendDays",
                schema: "schools",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "GenderPolicy",
                schema: "schools",
                table: "Campuses");

            migrationBuilder.DropColumn(
                name: "LocalName",
                schema: "schools",
                table: "Campuses");
        }
    }
}
