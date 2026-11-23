using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducationPortal.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class WholeSchoolRowPerCampus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<Guid>(
                name: "WholeSchoolKey",
                schema: "schools",
                table: "SchoolPhotos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WholeSchoolKey",
                schema: "academics",
                table: "Holidays",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WholeSchoolKey",
                schema: "academics",
                table: "CalendarEvents",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                -- Row-level security would hide other schools' rows from this migration, so it bypasses it for this transaction only (ADR-026).
                SELECT set_config('app.tenant_bypass', 'on', true);

                -- 1. Stop before changing anything when a row cannot be given a campus (no data is deleted by this migration).
                DO $$
                BEGIN
                  IF EXISTS (SELECT 1 FROM academics."TimetableSlots" WHERE "CampusId" IS NULL) THEN
                    RAISE EXCEPTION 'academics.TimetableSlots has rows without a campus. Give each one a campus, then run the migration again.';
                  END IF;
                  IF EXISTS (SELECT 1 FROM academics."Courses" WHERE "CampusId" IS NULL) THEN
                    RAISE EXCEPTION 'academics.Courses has rows without a campus. Give each one a campus, then run the migration again.';
                  END IF;
                  IF EXISTS (SELECT 1 FROM admissions."AdmissionEnquiries" WHERE "CampusId" IS NULL) THEN
                    RAISE EXCEPTION 'admissions.AdmissionEnquiries has rows without a campus. Give each one a campus, then run the migration again.';
                  END IF;
                  IF EXISTS (SELECT 1 FROM schools."ClassOfferings" WHERE "CampusId" IS NULL) THEN
                    RAISE EXCEPTION 'schools.ClassOfferings has rows without a campus. Give each one a campus, then run the migration again.';
                  END IF;
                  IF EXISTS (SELECT 1 FROM schools."SchoolReviews" WHERE "CampusId" IS NULL) THEN
                    RAISE EXCEPTION 'schools.SchoolReviews has rows without a campus. Give each one a campus, then run the migration again.';
                  END IF;
                  IF EXISTS (SELECT 1 FROM academics."Holidays" r WHERE r."CampusId" IS NULL
                      AND NOT EXISTS (SELECT 1 FROM schools."Campuses" c WHERE c."SchoolId" = r."SchoolId")) THEN
                    RAISE EXCEPTION 'academics.Holidays has whole-school rows of a school with no campus. Add a campus or delete those rows, then run the migration again.';
                  END IF;
                  IF EXISTS (SELECT 1 FROM academics."CalendarEvents" r WHERE r."CampusId" IS NULL
                      AND NOT EXISTS (SELECT 1 FROM schools."Campuses" c WHERE c."SchoolId" = r."SchoolId")) THEN
                    RAISE EXCEPTION 'academics.CalendarEvents has whole-school rows of a school with no campus. Add a campus or delete those rows, then run the migration again.';
                  END IF;
                  IF EXISTS (SELECT 1 FROM schools."SchoolPhotos" r WHERE r."CampusId" IS NULL
                      AND NOT EXISTS (SELECT 1 FROM schools."Campuses" c WHERE c."SchoolId" = r."SchoolId")) THEN
                    RAISE EXCEPTION 'schools.SchoolPhotos has whole-school rows of a school with no campus. Add a campus or delete those rows, then run the migration again.';
                  END IF;
                END $$;

                -- 2. Whole-school rows become one row per campus of their school (ADR-027). Rows with a campus are unchanged.
                -- academics."Holidays": the key of each whole-school row is derived from its id, so the copies of one row share it.
                UPDATE academics."Holidays" SET "WholeSchoolKey" = md5('Holidays' || "Id")::uuid WHERE "CampusId" IS NULL;

                -- One copy per campus of the school: every column is copied, the identity and public id are new, the campus is the campus's.
                INSERT INTO academics."Holidays"
                SELECT (jsonb_populate_record(NULL::academics."Holidays", to_jsonb(src) || jsonb_build_object(
                    'Id', nextval(pg_get_serial_sequence('academics."Holidays"', 'Id')),
                    'PublicId', gen_random_uuid(),
                    'CampusId', c."Id"))).*
                FROM academics."Holidays" src
                JOIN schools."Campuses" c ON c."SchoolId" = src."SchoolId"
                WHERE src."CampusId" IS NULL;

                DELETE FROM academics."Holidays" WHERE "CampusId" IS NULL;

                -- academics."CalendarEvents": the key of each whole-school row is derived from its id, so the copies of one row share it.
                UPDATE academics."CalendarEvents" SET "WholeSchoolKey" = md5('CalendarEvents' || "Id")::uuid WHERE "CampusId" IS NULL;

                -- One copy per campus of the school: every column is copied, the identity and public id are new, the campus is the campus's.
                INSERT INTO academics."CalendarEvents"
                SELECT (jsonb_populate_record(NULL::academics."CalendarEvents", to_jsonb(src) || jsonb_build_object(
                    'Id', nextval(pg_get_serial_sequence('academics."CalendarEvents"', 'Id')),
                    'PublicId', gen_random_uuid(),
                    'CampusId', c."Id"))).*
                FROM academics."CalendarEvents" src
                JOIN schools."Campuses" c ON c."SchoolId" = src."SchoolId"
                WHERE src."CampusId" IS NULL;

                DELETE FROM academics."CalendarEvents" WHERE "CampusId" IS NULL;

                -- schools."SchoolPhotos": the key of each whole-school row is derived from its id, so the copies of one row share it.
                UPDATE schools."SchoolPhotos" SET "WholeSchoolKey" = md5('SchoolPhotos' || "Id")::uuid WHERE "CampusId" IS NULL;

                -- One copy per campus of the school: every column is copied, the identity and public id are new, the campus is the campus's.
                INSERT INTO schools."SchoolPhotos"
                SELECT (jsonb_populate_record(NULL::schools."SchoolPhotos", to_jsonb(src) || jsonb_build_object(
                    'Id', nextval(pg_get_serial_sequence('schools."SchoolPhotos"', 'Id')),
                    'PublicId', gen_random_uuid(),
                    'CampusId', c."Id"))).*
                FROM schools."SchoolPhotos" src
                JOIN schools."Campuses" c ON c."SchoolId" = src."SchoolId"
                WHERE src."CampusId" IS NULL;

                DELETE FROM schools."SchoolPhotos" WHERE "CampusId" IS NULL;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "CampusId",
                schema: "academics",
                table: "TimetableSlots",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CampusId",
                schema: "schools",
                table: "SchoolReviews",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CampusId",
                schema: "schools",
                table: "SchoolPhotos",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CampusId",
                schema: "academics",
                table: "Holidays",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CampusId",
                schema: "academics",
                table: "Courses",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CampusId",
                schema: "schools",
                table: "ClassOfferings",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CampusId",
                schema: "academics",
                table: "CalendarEvents",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CampusId",
                schema: "admissions",
                table: "AdmissionEnquiries",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchoolPhotos_SchoolId_WholeSchoolKey",
                schema: "schools",
                table: "SchoolPhotos",
                columns: new[] { "SchoolId", "WholeSchoolKey" });

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_SchoolId_WholeSchoolKey",
                schema: "academics",
                table: "Holidays",
                columns: new[] { "SchoolId", "WholeSchoolKey" });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_SchoolId_WholeSchoolKey",
                schema: "academics",
                table: "CalendarEvents",
                columns: new[] { "SchoolId", "WholeSchoolKey" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SchoolPhotos_SchoolId_WholeSchoolKey",
                schema: "schools",
                table: "SchoolPhotos");

            migrationBuilder.DropIndex(
                name: "IX_Holidays_SchoolId_WholeSchoolKey",
                schema: "academics",
                table: "Holidays");

            migrationBuilder.DropIndex(
                name: "IX_CalendarEvents_SchoolId_WholeSchoolKey",
                schema: "academics",
                table: "CalendarEvents");

            migrationBuilder.DropColumn(
                name: "WholeSchoolKey",
                schema: "schools",
                table: "SchoolPhotos");

            migrationBuilder.DropColumn(
                name: "WholeSchoolKey",
                schema: "academics",
                table: "Holidays");

            migrationBuilder.DropColumn(
                name: "WholeSchoolKey",
                schema: "academics",
                table: "CalendarEvents");

            migrationBuilder.AlterColumn<int>(
                name: "CampusId",
                schema: "academics",
                table: "TimetableSlots",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "CampusId",
                schema: "schools",
                table: "SchoolReviews",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "CampusId",
                schema: "schools",
                table: "SchoolPhotos",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "CampusId",
                schema: "academics",
                table: "Holidays",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "CampusId",
                schema: "academics",
                table: "Courses",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "CampusId",
                schema: "schools",
                table: "ClassOfferings",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "CampusId",
                schema: "academics",
                table: "CalendarEvents",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "CampusId",
                schema: "admissions",
                table: "AdmissionEnquiries",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
