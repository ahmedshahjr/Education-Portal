using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EducationPortal.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class StudentProfilesAndModuleCodeEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The module code was a string (for example 'FEES'); it is now the number of a ModuleCode member. PostgreSQL needs an
            // explicit mapping for the cast. A row whose code is not one of the 17 built-in modules (a custom module made while the
            // catalogue was editable) maps to NULL and stops the migration: remove such a row first.
            migrationBuilder.Sql(
                "ALTER TABLE modules.\"Modules\" ALTER COLUMN \"Code\" TYPE integer USING (CASE \"Code\" WHEN 'SCHOOL' THEN 1 WHEN 'CAMPUS' THEN 2 WHEN 'ACADEMIC_SETUP' THEN 3 WHEN 'USERS_ROLES' THEN 4 WHEN 'STUDENTS' THEN 5 WHEN 'STAFF' THEN 6 WHEN 'ADMISSIONS' THEN 7 WHEN 'ATTENDANCE' THEN 8 WHEN 'FEES' THEN 9 WHEN 'PARENT_COMMUNICATION' THEN 10 WHEN 'EXAMS' THEN 11 WHEN 'TIMETABLE' THEN 12 WHEN 'HR_PAYROLL' THEN 13 WHEN 'TRANSPORT' THEN 14 WHEN 'LIBRARY' THEN 15 WHEN 'LMS' THEN 16 WHEN 'HOSTEL' THEN 17 END);");

            migrationBuilder.CreateTable(
                name: "Genders",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModuleCodes",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentProfiles",
                schema: "schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    UserDetailsId = table.Column<int>(type: "integer", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    FatherName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nationality = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ClassApplied = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PreviousSchool = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GuardianPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AdmissionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProfiles", x => x.Id);
                    table.UniqueConstraint("AK_StudentProfiles_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_StudentProfiles_Genders_Gender",
                        column: x => x.Gender,
                        principalSchema: "lookups",
                        principalTable: "Genders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentProfiles_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentProfiles_UserDetails_UserDetailsId",
                        column: x => x.UserDetailsId,
                        principalSchema: "schools",
                        principalTable: "UserDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "Genders",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Male" },
                    { 2, "Female" },
                    { 3, "Other" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "ModuleCodes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 0, "None" },
                    { 1, "School" },
                    { 2, "Campus" },
                    { 3, "AcademicSetup" },
                    { 4, "UsersRoles" },
                    { 5, "Students" },
                    { 6, "Staff" },
                    { 7, "Admissions" },
                    { 8, "Attendance" },
                    { 9, "Fees" },
                    { 10, "ParentCommunication" },
                    { 11, "Exams" },
                    { 12, "Timetable" },
                    { 13, "HrPayroll" },
                    { 14, "Transport" },
                    { 15, "Library" },
                    { 16, "Lms" },
                    { 17, "Hostel" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "UserTypes",
                columns: new[] { "Id", "Name" },
                values: new object[] { 4, "Student" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_Gender",
                schema: "schools",
                table: "StudentProfiles",
                column: "Gender");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_SchoolId",
                schema: "schools",
                table: "StudentProfiles",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_UserDetailsId",
                schema: "schools",
                table: "StudentProfiles",
                column: "UserDetailsId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Modules_ModuleCodes_Code",
                schema: "modules",
                table: "Modules",
                column: "Code",
                principalSchema: "lookups",
                principalTable: "ModuleCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Modules_ModuleCodes_Code",
                schema: "modules",
                table: "Modules");

            migrationBuilder.DropTable(
                name: "ModuleCodes",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "StudentProfiles",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "Genders",
                schema: "lookups");

            migrationBuilder.DeleteData(
                schema: "lookups",
                table: "UserTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.Sql(
                "ALTER TABLE modules.\"Modules\" ALTER COLUMN \"Code\" TYPE character varying(50) USING (CASE \"Code\" WHEN 1 THEN 'SCHOOL' WHEN 2 THEN 'CAMPUS' WHEN 3 THEN 'ACADEMIC_SETUP' WHEN 4 THEN 'USERS_ROLES' WHEN 5 THEN 'STUDENTS' WHEN 6 THEN 'STAFF' WHEN 7 THEN 'ADMISSIONS' WHEN 8 THEN 'ATTENDANCE' WHEN 9 THEN 'FEES' WHEN 10 THEN 'PARENT_COMMUNICATION' WHEN 11 THEN 'EXAMS' WHEN 12 THEN 'TIMETABLE' WHEN 13 THEN 'HR_PAYROLL' WHEN 14 THEN 'TRANSPORT' WHEN 15 THEN 'LIBRARY' WHEN 16 THEN 'LMS' WHEN 17 THEN 'HOSTEL' END);");
        }
    }
}
